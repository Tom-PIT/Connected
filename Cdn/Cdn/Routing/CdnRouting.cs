using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Cdn.Routing
{
	internal static class CdnRouting
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapControllerRoute("sys.data", "data", new { controller = "Data", action = "Notify" });
		}
	}
}