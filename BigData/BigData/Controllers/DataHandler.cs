using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System.Net;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;

namespace TomPIT.BigData.Controllers
{
    internal class DataHandler : MicroServiceContext
    {
        private readonly bool _successfullyInitialized = false;
        public DataHandler(HttpContext context)
        {
            Context = context;

            var ms = context.GetRouteValue("microService");
            var partition = context.GetRouteValue("partition");

            var microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(ms.ToString());

            if (microService == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            Configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(microService.Token, ComponentCategories.BigDataPartition, partition.ToString()) as IPartitionConfiguration;

            if (Configuration == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            MicroService = microService;

            Initialize();

            Body = Context.Request.Body.ToType<JArray>();
            _successfullyInitialized = true;
        }

        private IPartitionConfiguration Configuration { get; }
        private HttpContext Context { get; }
        private JArray Body { get; set; }

        public void ProcessRequest()
        {
            if (!_successfullyInitialized)
                return;

            MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Prepare(Configuration, Body);
        }
    }
}
