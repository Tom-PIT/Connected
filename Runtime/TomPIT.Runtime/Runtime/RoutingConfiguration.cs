﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using TomPIT.Routing;

namespace TomPIT.Runtime
{
	internal static class RoutingConfiguration
	{
		public static void Register(IEndpointRouteBuilder routes)
		{
			var statusController = "Status";

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				statusController = "MultiTenantStatus";

			routes.MapControllerRoute("sys.status", "sys/status/{code}", new { controller = statusController, action = "Index" });

			var loginController = "Login";

			if (Shell.GetService<IRuntimeService>().Environment == Runtime.RuntimeEnvironment.MultiTenant)
				loginController = "MultiTenantLogin";

			routes.MapControllerRoute("login", "login", new { controller = loginController, action = "Index" });
			routes.MapControllerRoute("login.authenticate", "login/authenticate", new { controller = loginController, action = "Authenticate" });
			routes.MapControllerRoute("login.changepassword", "login/change-password", new { controller = loginController, action = "ChangePassword" });
			routes.MapControllerRoute("logoff", "logoff", new { controller = loginController, action = "Logoff" });

			routes.Map("sys/avatar/{token}/{version}", async (t) =>
			{
				new AvatarRouteHandler().ProcessRequest(t);

				await Task.CompletedTask;
			});

			routes.Map("sys/deploy", async (t) =>
			{
				new DeployRouteHandler().ProcessRequest(t);

				await Task.CompletedTask;
			});

			routes.Map("sys/debug/{action}", async (t) =>
			{
				new DebugRouteHandler().ProcessRequest(t);

				await Task.CompletedTask;
			});
		}
	}
}