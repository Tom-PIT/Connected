using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace TomPIT.Rest.Controllers
{
	internal abstract class ApiFormatter
	{
		public HttpContext Context { get; set; }
		public JObject ParseArguments()
		{
			return OnParseArguments();
		}

		protected abstract JObject OnParseArguments();

		public async Task RenderError(int statusCode, string message)
		{
			await OnRenderError(statusCode, message);
		}

		protected abstract Task OnRenderError(int statusCode, string message);

		public async Task RenderResult(object content)
		{
			await OnRenderResult(content);
		}

		protected abstract Task OnRenderResult(object content);
	}
}
