using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System.Net;
using TomPIT.BigData.Services;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Services;

namespace TomPIT.BigData.Controllers
{
	internal class ApiHandler : ExecutionContext
	{
		public ApiHandler(HttpContext context)
		{
			Context = context;

			var ms = context.GetRouteValue("microService");
			var api = context.GetRouteValue("api");

			var microService = Instance.Connection.GetService<IMicroServiceService>().Select(ms.ToString());

			if (microService == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Api = Instance.GetService<IComponentService>().SelectConfiguration(microService.Token, "BigDataApi", api.ToString()) as IBigDataApi;

			if (Api == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Initialize(Instance.Connection.Url, microService);

			Body = Context.Request.Body.ToType<JArray>();
		}

		private IBigDataApi Api { get; }
		private HttpContext Context { get; }
		private JArray Body { get; set; }

		public void ProcessRequest()
		{
			Instance.GetService<ITransactionService>().Prepare(Api, Body);
		}
	}
}
