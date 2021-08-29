using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.Rest.Controllers
{
	internal class JsonApiFormatter : ApiFormatter
	{
		public const string ContentType = "application/json";
		protected override JObject OnParseArguments()
		{
			return Context.Request.Body.ToJObject();
		}

		protected override async Task OnRenderError(int statusCode, string message)
		{
			Context.Response.ContentType = ContentType;
			Context.Response.StatusCode = statusCode;

			var buffer = Encoding.UTF8.GetBytes(Serializer.Serialize(new
			{
				Message = message
			}));

			Context.Response.ContentLength = buffer.Length;
			await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);

			await Context.Response.CompleteAsync();
		}

		protected override async Task OnRenderResult(object content)
		{
			var buffer = content == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(Serializer.Serialize(content));

			Context.Response.Clear();
			Context.Response.ContentLength = buffer.Length;
			Context.Response.ContentType = ContentType;
			Context.Response.StatusCode = StatusCodes.Status200OK;

			await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);

			await Context.Response.CompleteAsync();
		}
	}
}
