using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Search.Routing
{
	internal static class SearchRouting
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapControllerRoute("default", "{controller}/{action}");
		}
	}
}
