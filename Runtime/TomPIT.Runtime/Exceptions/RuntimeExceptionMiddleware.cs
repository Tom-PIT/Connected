using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TomPIT.Exceptions
{
	public class RuntimeExceptionMiddleware : ExceptionMiddleware
	{
		public RuntimeExceptionMiddleware(RequestDelegate next) : base(next)
		{
		}

		protected override async Task HandleException(HttpContext context, Exception ex)
		{
			context.Response.Clear();
			context.Response.StatusCode = 500;
			context.Response.ContentType = "application/json";

			var jsonEx = new JObject
					{
						{ "source", ex.Source },
						{ "message", ex.Message }
					};

			await context.Response.WriteAsync(Types.Serialize(jsonEx));
		}
	}
}