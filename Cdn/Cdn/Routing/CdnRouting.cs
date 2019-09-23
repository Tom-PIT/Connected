using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Cdn.Routing
{
	internal static class CdnRouting
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
		}
	}
}