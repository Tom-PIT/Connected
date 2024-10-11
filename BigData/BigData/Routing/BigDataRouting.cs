using System;
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
			builder.MapPingRoute();
			builder.MapControllerRoute("sys.tracing.endpoints", "sys/tracing/endpoints", new { controller = "Tracing", action = "Endpoints" });

			builder.Map("data/{microService}/{partition}", (t) =>
			{
				using var handler = new DataHandler(t);

				handler.ProcessRequest();

				GC.Collect();
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
