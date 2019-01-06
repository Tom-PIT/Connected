using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using TomPIT.Routing;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT
{
	internal static class RoutingConfiguration
	{
		public static void Register(IRouteBuilder routes)
		{
			var statusController = "Status";
			var loginController = "Login";

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
			{
				statusController = "MultiTenantStatus";
				loginController = "MultiTenantLogin";
			}

			routes.MapRoute("sys.status", "sys/status/{code}", new { controller = statusController, action = "Index" });
			routes.MapRoute("login", "sys/login", new { controller = loginController, action = "Index" });
			routes.MapRoute("logoff", "sys/logoff", new { controller = loginController, action = "Logoff" });
			routes.MapRoute("login.authenticate", "sys/login/authenticate", new { controller = loginController, action = "Authenticate" });
			routes.MapRoute("login.changepassword", "sys/login/change-password", new { controller = loginController, action = "ChangePassword" });

			routes.MapRoute("sys/avatar/{token}/{version}", (t) =>
			{
				new AvatarRouteHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}