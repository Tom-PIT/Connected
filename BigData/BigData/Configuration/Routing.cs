using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using TomPIT.BigData.Controllers;

namespace TomPIT.BigData.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder builder)
		{
			builder.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });

			builder.MapRoute("data/{microService}/{partition}", (t) =>
			{
				var handler = new DataHandler(t);

				handler.ProcessRequest();

				return Task.CompletedTask;
			});
		}
	}
}
