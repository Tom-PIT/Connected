﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Routing;

namespace TomPIT.Runtime
{
	internal static class RoutingConfiguration
	{
		public static void Register(IRouteBuilder routes)
		{
			var statusController = "Status";

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				statusController = "MultiTenantStatus";

			routes.MapRoute("sys.status", "sys/status/{code}", new { controller = statusController, action = "Index" });

			routes.AddSystemLogin();

			routes.MapRoute("sys/avatar/{token}/{version}", (t) =>
			{
				new AvatarRouteHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}