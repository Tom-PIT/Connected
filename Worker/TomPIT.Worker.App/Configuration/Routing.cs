using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Worker.Configuration
{
	internal static class Routing
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
            routes.MapControllerRoute("sys.dispatchers", "sys/dispatchers", new { controller = "Ping", action = "Dispatchers" });
        }
	}
}