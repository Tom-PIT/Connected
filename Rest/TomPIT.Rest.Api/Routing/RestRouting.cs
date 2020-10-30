using System.Threading.Tasks;
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

			routes.Map("{microservice}/{api}/{operation}", (t) =>
			{
				new ApiHandler(t).Invoke();

				return Task.CompletedTask;
			});
		}
	}
}