using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using TomPIT.Reflection;

namespace TomPIT.Rest.Controllers
{
	internal class FormApiFormatter : ApiFormatter
	{
		public const string ContentType = "application/x-www-form-urlencoded";
		protected override JObject OnParseArguments()
		{
			var body = new StreamReader(Context.Request.Body, Encoding.UTF8).ReadToEndAsync().Result;
			var qs = QueryHelpers.ParseNullableQuery(body);
			var result = new JObject();

			foreach (var q in qs)
				result.Add(new JProperty(q.Key, q.Value.ToString()));

			Context.SetRequestArguments(result);

			return result;
		}

		protected override async Task OnRenderResult(object content)
		{
			var qs = new QueryString();

			if (content != null)
			{
				foreach (var property in content.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (!property.PropertyType.IsTypePrimitive())
						continue;

					qs.Add(property.Name.ToCamelCase(), property.GetValue(content) as string);
				}
			}

			var buffer = Encoding.UTF8.GetBytes(qs.ToUriComponent());

			Context.Response.Clear();
			Context.Response.ContentLength = buffer.Length;
			Context.Response.ContentType = ContentType;
			Context.Response.StatusCode = StatusCodes.Status200OK;

			await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
		}

		protected override async Task OnRenderError(int statusCode, string message)
		{
			Context.Response.ContentType = ContentType;
			Context.Response.StatusCode = statusCode;

			var qs = new QueryString();

			qs.Add("message", message);

			var buffer = Encoding.UTF8.GetBytes(qs.ToUriComponent());

			Context.Response.ContentLength = buffer.Length;
			await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
		}
	}
}
