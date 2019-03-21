using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace TomPIT.Rest.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });

			routes.MapRoute("{microservice}/{api}/{operation}", (t) =>
			{
				new ApiHandler(t, Instance.Connection.Url).Invoke();

				return Task.CompletedTask;
			});
		}
	}
}