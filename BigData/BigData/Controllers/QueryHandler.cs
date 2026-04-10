using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;
using TomPIT.Proxy;
using TomPIT.Serialization;

namespace TomPIT.BigData.Controllers
{
    internal class QueryHandler : MicroServiceContext
    {
        private readonly bool _successfullyInitialized = false;

        public QueryHandler(HttpContext context)
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

        public async Task ProcessRequestAsync()
        {
            if (!_successfullyInitialized)
                return;

            var result = MiddlewareDescriptor.Current.Tenant.GetService<IBigDataProxy>()
                .Query(Configuration, Body);

            if (result != null)
            {
                var content = Serializer.Serialize(result);
                var buffer = Encoding.UTF8.GetBytes(content);

                Shell.HttpContext.Response.Clear();
                Shell.HttpContext.Response.ContentLength = buffer.Length;
                Shell.HttpContext.Response.ContentType = "application/json";
                Shell.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

                await Shell.HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                await Shell.HttpContext.Response.CompleteAsync();
            }
        }

        private IPartitionConfiguration Configuration { get; }
        private HttpContext Context { get; }
        private JArray Body { get; set; }
    }
}
