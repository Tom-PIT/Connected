using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Search.Routing
{
	internal static class SearchRouting
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapRoute("default", "{controller}/{action}");
		}
	}
}
