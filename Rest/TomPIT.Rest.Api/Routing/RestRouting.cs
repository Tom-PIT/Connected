using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Rest.Controllers;

namespace TomPIT.Rest.Routing
{
	internal static class RestRouting
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });

			routes.Map("{microservice}/{api}/{operation}", async (t) =>
			{
				using var handler = new ApiHandler(t);

				await handler.Invoke();
			});
		}
	}
}