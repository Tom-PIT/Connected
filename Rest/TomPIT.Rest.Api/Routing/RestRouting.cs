using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Rest.Controllers;

namespace TomPIT.Rest.Routing
{
	internal static class RestRouting
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });

			routes.MapRoute("{microservice}/{api}/{operation}", (t) =>
			{
				new ApiHandler(t, Instance.Tenant.Url).Invoke();

				return Task.CompletedTask;
			});
		}
	}
}