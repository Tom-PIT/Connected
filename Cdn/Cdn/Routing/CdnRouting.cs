using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Cdn.Routing
{
	internal static class CdnRouting
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
            routes.MapControllerRoute("sys.dispatchers", "sys/dispatchers", new { controller = "Ping", action = "Dispatchers" });
            routes.MapControllerRoute("sys.printingSpooler", "sys/printing-spooler", new { controller = "PrintingSpooler", action = "SelectJob" });
			routes.MapControllerRoute("sys.localization.localize", "sys/localization/localize", new { controller = "Localization", action = "Localize" });
			routes.MapControllerRoute("sys.client.notify", "sys/clients/notify", new { controller = "Client", action = "Notify" });
		}
	}
}