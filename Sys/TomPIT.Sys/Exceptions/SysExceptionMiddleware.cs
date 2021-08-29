using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TomPIT.Sys.Exceptions
{
	internal class SysExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public SysExceptionMiddleware(RequestDelegate next)
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

				await HandleException(context, ex);
			}
		}

		protected virtual async Task OnHandleAjaxException(HttpContext context, Exception ex)
		{
			await Task.CompletedTask;

			throw ex;
		}

		protected virtual async Task OnHandleException(HttpContext context, Exception ex)
		{
			await Task.CompletedTask;

			context.Response.Clear();
			context.Response.StatusCode = 500;
			context.Response.ContentType = "application/json";

			var jsonEx = new JObject
					{
						{ "source", ex.Source },
						{ "message", ex.Message }
					};

			await context.Response.WriteAsync(JsonConvert.SerializeObject(jsonEx));

			await context.Response.CompleteAsync();
		}

		protected virtual async Task HandleException(HttpContext context, Exception ex)
		{
			if (context.Request.IsAjaxRequest())
				await OnHandleAjaxException(context, ex);
			else
				await OnHandleException(context, ex);
		}
	}
}