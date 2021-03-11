using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.BigData.Controllers;

namespace TomPIT.BigData.Configuration
{
	internal static class Routing
	{
		public static void Register(IEndpointRouteBuilder builder)
		{
			builder.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });

			builder.Map("data/{microService}/{partition}", (t) =>
			{
				using var handler = new DataHandler(t);

				handler.ProcessRequest();

				return Task.CompletedTask;
			});

			builder.Map("query/{microService}/{partition}", (t) =>
			{
				using var handler = new QueryHandler(t);

				handler.ProcessRequest();

				return Task.CompletedTask;
			});
		}
	}
}
