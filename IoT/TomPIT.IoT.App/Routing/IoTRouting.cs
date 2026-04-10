using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.IoT.Controllers;

namespace TomPIT.IoT.Routing
{
    internal static class IoTRouting
    {
        public static void Register(IEndpointRouteBuilder routes)
        {
            routes.MapPingRoute();

            routes.Map("transaction/{microService}/{hub}/{device}/{transaction}", async (t) =>
            {
                using var handler = new TransactionHandler(t);

                await handler.ProcessRequestAsync();
            });
        }
    }
}