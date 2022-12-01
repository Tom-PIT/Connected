using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.ExceptionServices;
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

				var exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);

				await HandleException(context, exceptionDispatchInfo);
			}
		}

		protected virtual async Task OnHandleAjaxException(HttpContext context, ExceptionDispatchInfo ex)
		{
			await Task.CompletedTask;

			ex.Throw();
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

		protected virtual async Task HandleException(HttpContext context, ExceptionDispatchInfo ex)
		{
			if (context.Request.IsAjaxRequest())
				await OnHandleAjaxException(context, ex);
			else
				await OnHandleException(context, ex.SourceException);
		}
	}
}