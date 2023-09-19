using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Exceptions
{
	public class AjaxExceptionMiddleware : ExceptionMiddleware
	{
		public AjaxExceptionMiddleware(RequestDelegate next) : base(next)
		{
		}

		private HttpStatusCode ResolveStatusCode(Exception ex)
		{
			//If wrapped, unwrap
			if (ex is TomPITException tpEx)
			{
				if (tpEx.InnerException is not null)
					return ResolveStatusCode(tpEx.InnerException);
			}
			else if (ex is BadRequestException)
				return HttpStatusCode.BadRequest;
			else if (ex is MiddlewareValidationException)
				return HttpStatusCode.BadRequest;
			else if (ex is UnauthorizedException)
				return HttpStatusCode.Unauthorized;
			else if (ex is AuthenticationException)
				return HttpStatusCode.Unauthorized;

			return HttpStatusCode.InternalServerError;
		}

		protected override async Task OnHandleAjaxException(HttpContext context, Exception ex)
		{
			if (context.Response.StatusCode == (int)HttpStatusCode.OK || context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
				context.Response.StatusCode = (int)ResolveStatusCode(ex);

			context.Response.ContentType = "application/json";

			var severity = ExceptionSeverity.Critical;
			var source = ex.Source;
			var message = ex.Message;

			if (ex is RuntimeException)
			{
				if (ex.InnerException != null && ex.InnerException is UnauthorizedException)
				{
					severity = ExceptionSeverity.Warning;
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

					message = ex.InnerException.Message;
				}
				else
				{
					severity = ((RuntimeException)ex).Severity;

					if (ex is ScriptException script)
					{
						source = $"{script.Path} ({script.Line})";
					}
				}
			}
			else if (ex.Data != null && ex.Data.Count > 0)
			{
				var script = ex.Data.Contains("Script") ? ex.Data["Script"].ToString() : string.Empty;
				var ms = ex.Data.Contains("MicroService") ? ex.Data["MicroService"] : null;
				var msName = ms == null ? string.Empty : (ms as IMicroService).Name;

				if (!string.IsNullOrWhiteSpace(script))
					source = $"{msName}/{script}";
			}

			if (ex is ValidationAggregateException || ex is System.ComponentModel.DataAnnotations.ValidationException)
			{
				var ms = ex.Data.Contains("MicroService") ? ex.Data["MicroService"] as IMicroService : null;

				if (ms != null && ms.Status != MicroServiceStatus.Production && ex.Data.Contains("Script"))
					source = $"{ms.Name}/{ex.Data["Script"].ToString()}";

				severity = ExceptionSeverity.Warning;
			}

			var jsonEx = new JObject
						  {
								{ "source", source },
								{ "message", ex.Message },
								{ "severity", severity.ToString().ToLower() }
						  };


			if (Middleware.MiddlewareDescriptor.Current?.Tenant?.GetService<IRuntimeService>() is IRuntimeService runtimeService)
			{
				if (runtimeService.Stage == EnvironmentStage.Development)
				{
					jsonEx["stack"] = ex.StackTrace;
				}
			}

			await context.Response.WriteAsync(Serializer.Serialize(jsonEx));

			await context.Response.CompleteAsync();

			return;
		}
	}
}