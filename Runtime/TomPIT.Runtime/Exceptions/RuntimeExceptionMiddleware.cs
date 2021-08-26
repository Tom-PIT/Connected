using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

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

			if (ex is UnauthorizedException ua)
				context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			else if (ex is NotFoundException nf)
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			else if (ex is ForbiddenException fb)
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			else
				context.Response.StatusCode = 500;

			context.Response.ContentType = "application/json";

			var jsonEx = new JObject
					{
						{ "source", ex.Source },
						{ "message", ex.Message }
					};

			await context.Response.WriteAsync(Serializer.Serialize(jsonEx));

			await context.Response.CompleteAsync();
		}
	}
}