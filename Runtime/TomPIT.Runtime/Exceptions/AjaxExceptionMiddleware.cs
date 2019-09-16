using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
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
			context.Response.StatusCode = 500;
			context.Response.ContentType = "application/json";

			var severity = ExceptionSeverity.Critical;

			if (ex is RuntimeException)
			{
				severity = ((RuntimeException)ex).Severity;
			}

			var jsonEx = new JObject
					{
						{ "source", ex.Source },
						{ "message", ex.Message },
						{ "severity", severity.ToString().ToLower() }
					};

			await context.Response.WriteAsync(SerializationExtensions.Serialize(jsonEx));

			return;
		}
	}
}