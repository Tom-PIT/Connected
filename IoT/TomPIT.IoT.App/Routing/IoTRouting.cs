using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.IoT.Routing
{
	internal static class IoTRouting
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
		}
	}
}