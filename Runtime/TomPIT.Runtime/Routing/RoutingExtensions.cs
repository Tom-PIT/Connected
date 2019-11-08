using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.Runtime;

namespace TomPIT.Routing
{
	public static class RoutingExtensions
	{
		public static void RemoveSystemLogin(this IRouteBuilder builder)
		{
			var toRemove = new List<Microsoft.AspNetCore.Routing.Route>();

			foreach (var route in builder.Routes)
			{
				if (!(route is Microsoft.AspNetCore.Routing.Route r) || string.IsNullOrWhiteSpace(r.Name))
					continue;

				if (r.Name.StartsWith("login"))
					toRemove.Add(r);
			}

			foreach (var route in toRemove)
				builder.Routes.Remove(route);
		}

		public static void AddSystemLogin(this IRouteBuilder builder)
		{
			foreach (var route in builder.Routes)
			{
				if (!(route is Microsoft.AspNetCore.Routing.Route r) || string.IsNullOrWhiteSpace(r.Name))
					continue;

				if (r.Name.StartsWith("login"))
					return;
			}

			var loginController = "Login";

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				loginController = "MultiTenantLogin";

			builder.MapRoute("login", "login", new { controller = loginController, action = "Index" });
			builder.MapRoute("login.authenticate", "login/authenticate", new { controller = loginController, action = "Authenticate" });
			builder.MapRoute("login.changepassword", "login/change-password", new { controller = loginController, action = "ChangePassword" });

			foreach (var route in builder.Routes)
			{
				if (route is Microsoft.AspNetCore.Routing.Route r && !string.IsNullOrEmpty(r.Name) && string.Compare(r.Name, "logoff", true) == 0)
					return;
			}

			builder.MapRoute("logoff", "logoff", new { controller = loginController, action = "Logoff" });
		}
	}
}
