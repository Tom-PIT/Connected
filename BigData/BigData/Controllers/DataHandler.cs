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
	internal class DataHandler : ExecutionContext
	{
		public DataHandler(HttpContext context)
		{
			Context = context;

			var ms = context.GetRouteValue("microService");
			var partition = context.GetRouteValue("partition");

			var microService = Instance.Connection.GetService<IMicroServiceService>().Select(ms.ToString());

			if (microService == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Configuration = Instance.GetService<IComponentService>().SelectConfiguration(microService.Token, "BigDataPartition", partition.ToString()) as IPartitionConfiguration;

			if (Configuration == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Initialize(Instance.Connection.Url, microService);

			Body = Context.Request.Body.ToType<JArray>();
		}

		private IPartitionConfiguration Configuration { get; }
		private HttpContext Context { get; }
		private JArray Body { get; set; }

		public void ProcessRequest()
		{
			Instance.GetService<ITransactionService>().Prepare(Configuration, Body);
		}
	}
}
