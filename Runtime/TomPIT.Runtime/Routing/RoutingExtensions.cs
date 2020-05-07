using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using TomPIT.Runtime;

namespace TomPIT.Routing
{
	public static class RoutingExtensions
	{
		public static void RemoveSystemLogin(this IEndpointRouteBuilder builder)
		{
			var toRemove = new List<Microsoft.AspNetCore.Routing.Route>();

			//foreach (var route in builder.Routes)
			//{
			//	if (!(route is Microsoft.AspNetCore.Routing.Route r) || string.IsNullOrWhiteSpace(r.Name))
			//		continue;

			//	if (r.Name.StartsWith("login"))
			//		toRemove.Add(r);
			//}

			//foreach (var route in toRemove)
			//	builder.Routes.Remove(route);

			//builder.ApplicationBuilder.
			//MiddlewareDescriptor.Current.Tenant.GetService<IRuntimeService>().Host.UseRouter(builder.Build());
			//MiddlewareDescriptor.Current.Tenant.GetService<IRuntimeService>().Host.UseRouting();
		}

		public static void AddSystemLogin(this IEndpointRouteBuilder builder)
		{
			return;
			foreach (var dataSource in builder.DataSources)
			{
				foreach (var endpoint in dataSource.Endpoints)
				{

				}
			}
			return;
			//foreach (var route in builder.Routes)
			//{
			//	if (!(route is Microsoft.AspNetCore.Routing.Route r) || string.IsNullOrWhiteSpace(r.Name))
			//		continue;

			//	if (r.Name.StartsWith("login"))
			//		return;
			//}

			var loginController = "Login";

			if (Shell.GetService<IRuntimeService>().Environment == Runtime.RuntimeEnvironment.MultiTenant)
				loginController = "MultiTenantLogin";

			//builder.MapControllerRoute("login", "login", new { controller = loginController, action = "Index" });
			//builder.MapControllerRoute("login.authenticate", "login/authenticate", new { controller = loginController, action = "Authenticate" });
			//builder.MapControllerRoute("login.changepassword", "login/change-password", new { controller = loginController, action = "ChangePassword" });

			foreach (var dataSource in builder.DataSources)
			{
				foreach (var endpoint in dataSource.Endpoints)
				{
					//if (route is Microsoft.AspNetCore.Routing.Route r && !string.IsNullOrEmpty(r.Name) && string.Compare(r.Name, "logoff", true) == 0)
					//	return;
				}
			}

			//builder.MapControllerRoute("logoff", "logoff", new { controller = loginController, action = "Logoff" });
		}
	}
}
