using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;

namespace TomPIT.BigData.Controllers
{
	internal class DataHandler : MicroServiceContext
	{
		public DataHandler(HttpContext context)
		{
			Context = context;

			var ms = context.GetRouteValue("microService");
			var partition = context.GetRouteValue("partition");

			var microService = Instance.Tenant.GetService<IMicroServiceService>().Select(ms.ToString());

			if (microService == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Configuration = Instance.Tenant.GetService<IComponentService>().SelectConfiguration(microService.Token, "BigDataPartition", partition.ToString()) as IPartitionConfiguration;

			if (Configuration == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			MicroService = microService;

			Initialize(Instance.Tenant.Url);

			Body = Context.Request.Body.ToType<JArray>();
		}

		private IPartitionConfiguration Configuration { get; }
		private HttpContext Context { get; }
		private JArray Body { get; set; }

		public void ProcessRequest()
		{
			Instance.Tenant.GetService<ITransactionService>().Prepare(Configuration, Body);
		}
	}
}
