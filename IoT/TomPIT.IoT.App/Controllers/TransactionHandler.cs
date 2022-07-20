using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.IoT.Hubs;
using TomPIT.Middleware;
using Microsoft.Extensions.DependencyInjection;


namespace TomPIT.IoT.Controllers
{
	internal class TransactionHandler : MicroServiceContext
	{
		public TransactionHandler(HttpContext context)
		{
			Context = context;

			Hub = context.GetRouteValue("hub").ToString();
			Device = context.GetRouteValue("device").ToString();
			Transaction = context.GetRouteValue("transaction").ToString();

			var microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(context.GetRouteValue("microService").ToString());

			if (microService is null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(microService.Token, ComponentCategories.IoTHub, Hub) as IIoTHubConfiguration;

			if (Configuration is null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			MicroService = microService;

			Initialize(MiddlewareDescriptor.Current.Tenant.Url);

			Body = Context.Request.Body.ToJObject();
		}

		public void ProcessRequest()
		{
            var payload = new JObject
            {
                { "arguments", Body },
                { "transaction", Transaction },
                { "device", $"{MicroService.Name}/{Hub}/{Device}" }
            };

            IoTServerHub.Invoke(payload, HubContext?.Clients).Wait();
			
			Shell.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
			Shell.HttpContext.Response.CompleteAsync().Wait();
		}

		private string Hub { get; }
		private string Device { get; }
		private string Transaction { get; }

		private IIoTHubConfiguration Configuration { get; }
		private HttpContext Context { get; }

		private IHubContext<IoTServerHub> HubContext => Context.RequestServices.GetService<IHubContext<IoTServerHub>>();
		private JObject Body { get; set; }
	}
}
