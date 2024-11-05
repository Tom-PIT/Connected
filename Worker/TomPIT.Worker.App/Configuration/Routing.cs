using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Worker.Configuration
{
	internal static class Routing
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapPingRoute();
			routes.MapControllerRoute("sys.dispatchers", "sys/dispatchers", new { controller = "Ping", action = "Dispatchers" });
		}
	}
}