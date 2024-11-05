using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Rest.Controllers;

namespace TomPIT.Rest.Routing
{
    internal static class RestRouting
    {
        public static void Register(IEndpointRouteBuilder routes)
        {
            routes.MapPingRoute();

            routes.Map("rest/{microservice}/{api}/{operation}", async (t) =>
            {
                using var handler = new ApiHandler(t);

                await handler.Invoke();
            });
        }
    }
}