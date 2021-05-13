using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Models;
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
			if(context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
			{
				await ErrorResult(context, ex);
				return;
			}

			context.Response.Redirect($"{RuntimeExtensions.RootUrl}/sys/status/{context.Response.StatusCode}");

			await Task.CompletedTask;
		}

		private async Task ErrorResult(HttpContext context, Exception ex)
		{
			var r = new ViewResult
			{
				ViewName = "~/Views/Shell/Error.cshtml",
				StatusCode = 500,
				ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
			};

			r.ViewData.Model = new ExceptionModel(context, ex);
			r.ViewData.Add("exSource", ex.Source);

			if (Shell.GetService<IRuntimeService>().Stage != EnvironmentStage.Production)
				r.ViewData.Add("exStack", ex.StackTrace);

			var sb = new StringBuilder();
			var tenant = MiddlewareDescriptor.Current.Tenant;
			var devUrl = tenant.GetService<IInstanceEndpointService>().Url(InstanceType.Development, InstanceVerbs.All);

			if (ex is ScriptException script)
			{
				r.ViewData.Add("exPath", script.Path);

				if (!string.IsNullOrWhiteSpace(script.MicroService))
				{

					var ms = tenant.GetService<IMicroServiceService>().Select(script.MicroService);

					if (ms != null && ms.Status != MicroServiceStatus.Production)
					{
						if (tenant != null && !string.IsNullOrWhiteSpace(devUrl))
						{
							r.ViewData.Add("exUrl", $"{devUrl}/ide/{ms.Url}?component={script.Component}&element={script.Element}");
						}
					}
				}

				r.ViewData.Add("exLine", script.Line);
			}

			if (ex.Data != null && ex.Data.Count > 0)
			{
				if (ex.Data.Contains("Script"))
					r.ViewData.Add("exScript", ex.Data["Script"]);

				if (ex.Data.Contains("MicroService") && !string.IsNullOrWhiteSpace(devUrl))
				{
					var scriptMs = ex.Data["MicroService"] as IMicroService;
					//TODO: resolve fully qualified component to be able to parse edit url

					if (scriptMs != null)
						r.ViewData.Add("exScriptMicroService", scriptMs.Name);
					//	r.ViewData.Add("exScriptUrl", $"{devUrl}/ide/{scriptMs.Name}?component={script.Component}&element={script.Element}");
				}
			}

			sb.AppendLine(ex.Message);

			if (Shell.GetService<IRuntimeService>().Stage != EnvironmentStage.Production)
			{
				sb.Append(ex.StackTrace);

				if (ex is TomPITException tp)
					r.ViewData.Add("exDiagnosticTrace", tp.DiagnosticsTrace);
				else if (ex is MiddlewareValidationException mw)
					r.ViewData.Add("exDiagnosticTrace", mw.DiagnosticsTrace);
			}

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
				else if (ex is MiddlewareValidationException)
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				else
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
		}
	}
}