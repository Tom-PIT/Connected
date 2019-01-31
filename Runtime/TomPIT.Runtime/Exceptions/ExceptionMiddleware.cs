using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Diagnostics;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.Exceptions
{
	public abstract class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				if (context.Response.HasStarted)
					throw;

				LogException(ex);

				await HandleException(context, ex);
			}
		}

		private void LogException(Exception ex)
		{
			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				return;

			var e = new LogEntry
			{
				Category = "Unhandled",
				Created = DateTime.UtcNow,
				Level = System.Diagnostics.TraceLevel.Error,
				Source = ex.Source,
				Message = string.Format("{0}{1}{2}", ex.Message, System.Environment.NewLine, ex.StackTrace)
			};

			if (ex is RuntimeException rt)
			{
				e.Element = rt.Element;
				e.Component = rt.Component;
				e.EventId = rt.EventId;
				e.Metric = rt.Metric;
			}

			Instance.Connection.GetService<ILoggingService>().Write(e);
		}

		protected virtual async Task OnHandleAjaxException(HttpContext context, Exception ex)
		{
			await Task.CompletedTask;

			throw ex;
		}

		protected virtual async Task OnHandleException(HttpContext context, Exception ex)
		{
			var r = new ViewResult
			{
				ViewName = "~/Views/Shell/Error.cshtml",
				StatusCode = 500,
				ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
			};

			r.ViewData.Model = new ExceptionModel(context, ex);
			r.ViewData.Add("exSource", ex.Source);
			var sb = new StringBuilder();

			sb.AppendLine(ex.Message);

			if (Shell.GetService<IRuntimeService>().Stage != EnvironmentStage.Production)
				sb.Append(ex.StackTrace);

			r.ViewData.Add("exMessage", sb.ToString());

			var exec = context.RequestServices.GetRequiredService<IActionResultExecutor<ViewResult>>();
			var desc = new ActionDescriptor
			{

			};
			var action = new ActionContext(context, context.GetRouteData(), desc);

			await exec.ExecuteAsync(action, r);
		}

		protected virtual async Task HandleException(HttpContext context, Exception ex)
		{
			if (context.Request.IsAjaxRequest() || !Shell.GetService<IRuntimeService>().SupportsUI)
				await OnHandleAjaxException(context, ResolveException(ex));
			else
				await OnHandleException(context, ResolveException(ex));
		}

		private Exception ResolveException(Exception ex)
		{
			if (ex is TargetInvocationException)
				return ex.InnerException;

			return ex;
		}
	}
}