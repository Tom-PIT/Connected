using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Search.Routing
{
	internal static class SearchRouting
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapPingRoute();
			routes.MapControllerRoute("default", "{controller}/{action}");
		}
	}
}
