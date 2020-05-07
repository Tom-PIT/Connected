using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Serialization;

namespace TomPIT.Exceptions
{
	public class AjaxExceptionMiddleware : ExceptionMiddleware
	{
		public AjaxExceptionMiddleware(RequestDelegate next) : base(next)
		{
		}

		protected override async Task OnHandleAjaxException(HttpContext context, Exception ex)
		{
			if (context.Response.StatusCode == (int)HttpStatusCode.OK)
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			context.Response.ContentType = "application/json";

			var severity = ExceptionSeverity.Critical;
			var source = ex.Source;
			var message = ex.Message;

			if (ex is RuntimeException)
			{
				severity = ((RuntimeException)ex).Severity;

				if (ex is ScriptException script)
				{
					source = $"{script.MicroService}/{script.Path} ({script.Line})";
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

			if (ex is ValidationException || ex is System.ComponentModel.DataAnnotations.ValidationException)
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

			await context.Response.WriteAsync(Serializer.Serialize(jsonEx));

			return;
		}
	}
}