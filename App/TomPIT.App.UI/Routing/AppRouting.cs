using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TomPIT.App.Globalization;
using TomPIT.App.Resources;
using TomPIT.App.UI;
using TomPIT.App.UI.Theming;
using TomPIT.Middleware;
using TomPIT.Navigation;
using TomPIT.Routing;
using TomPIT.UI;

namespace TomPIT.App.Routing
{
	internal static class AppRouting
	{
		public static void Register(IRouteBuilder routes)
		{
			routes.MapRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapRoute("sys.api", "sys/api/invoke", new { controller = "Api", action = "Invoke" });
			routes.MapRoute("sys.search", "sys/api/search", new { controller = "Api", action = "Search" });
			routes.MapRoute("sys.partial", "sys/api/partial", new { controller = "Api", action = "Partial" });
			routes.MapRoute("sys.setuserdata", "sys/api/setuserdata", new { controller = "Api", action = "SetUserData" });
			routes.MapRoute("sys.getuserdata", "sys/api/getuserdata", new { controller = "Api", action = "GetUserData" });
			routes.MapRoute("sys.queryuserdata", "sys/api/queryuserdata", new { controller = "Api", action = "QueryUserData" });

			routes.MapRoute("sys/themes/{microService}/{theme}", (t) =>
			{
				new ThemeHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/globalize/{locale}/{segments}", (t) =>
			{
				new GlobalizationHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/bundles/{microService}/{bundle}", (t) =>
			{
				new BundleHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/media/{id}/{version}", (t) =>
			{
				new MediaHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.MapRoute("sys/mail-template/{token}", (t) =>
			{
				var ve = t.RequestServices.GetService(typeof(IMailTemplateViewEngine)) as MailTemplateViewEngine;

				ve.Context = t;

				ve.Render(new Guid(t.GetRouteValue("token").ToString()));

				return Task.CompletedTask;
			});

			routes.MapRoute("{*.}", (t) =>
			{
				if (string.IsNullOrWhiteSpace(t.Request.Path.ToString().Trim('/')))
					t.Request.Path = "/home";

				if (Redirect(t))
					return Task.CompletedTask;

				var ve = t.RequestServices.GetService(typeof(IViewEngine)) as ViewEngine;

				ve.Context = t;

				ve.Render(t.Request.Path);

				return Task.CompletedTask;
			});
		}

		private static bool Redirect(HttpContext context)
		{
			var routes = new RouteValueDictionary();

			if (MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(context.Request.Path, routes) is ISiteMapRedirectRoute redirect)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Redirect;
				context.Response.Redirect(redirect.RedirectUrl(routes));

				return true;
			}

			return false;
		}
	}
}
