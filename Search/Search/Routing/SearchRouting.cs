using Microsoft.AspNetCore.Routing;

namespace TomPIT.Search.Routing
{
	internal static class SearchRouting
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapPingRoute();
		}
	}
}
