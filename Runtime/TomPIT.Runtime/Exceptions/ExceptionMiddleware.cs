using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Runtime;

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

			if (ex is TomPITException tp)
			{
				if (tp.InnerException is MiddlewareValidationException val)
				{
					var valType = Shell.GetService<IRuntimeService>().Type;

					val.LogWarning($"{LogCategories.Unhandled} - {valType}");
					return;

				}

				var type = Shell.GetService<IRuntimeService>().Type;

				tp.LogError($"{LogCategories.Unhandled} - {type}");
				return;
			}
			else if (ex is MiddlewareValidationException validation)
			{
				var type = Shell.GetService<IRuntimeService>().Type;

				validation.LogWarning($"{LogCategories.Unhandled} - {type}");
				return;
			}

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

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(e);
		}

		protected virtual async Task OnHandleAjaxException(HttpContext context, Exception ex)
		{
			await Task.CompletedTask;

			throw ex;
		}

		protected virtual async Task OnHandleException(HttpContext context, Exception ex)
		{
			context.Response.Redirect($"/sys/status/{context.Response.StatusCode}");

			await Task.CompletedTask;
		}

		protected virtual async Task HandleException(HttpContext context, Exception ex)
		{
			var resolvedException = ResolveException(ex);

			SetResponseStatus(context, resolvedException);

			if (context.Request.IsAjaxRequest() || !Shell.GetService<IRuntimeService>().SupportsUI)
				await OnHandleAjaxException(context, resolvedException);
			else
				await OnHandleException(context, resolvedException);
		}

		private Exception ResolveException(Exception ex)
		{
			if (ex is TargetInvocationException)
				return ex.InnerException;

			return ex;
		}

		private void SetResponseStatus(HttpContext context, Exception ex)
		{
			if (ex is ScriptException script && script.InnerException != null)
				SetResponseStatus(context, script.InnerException);
			else
			{
				if (ex is ForbiddenException)
					context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				else if (ex is NotFoundException)
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				else if (ex is UnauthorizedException)
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
		}
	}
}