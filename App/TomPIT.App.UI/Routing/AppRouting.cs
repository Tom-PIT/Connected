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
		public static void Register(IApplicationBuilder app, IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapControllerRoute("sys.api", "sys/api/invoke", new { controller = "Api", action = "Invoke" });
			routes.MapControllerRoute("sys.search", "sys/api/search", new { controller = "Api", action = "Search" });
			routes.MapControllerRoute("sys.partial", "sys/api/partial", new { controller = "Api", action = "Partial" });
			routes.MapControllerRoute("sys.setuserdata", "sys/api/setuserdata", new { controller = "Api", action = "SetUserData" });
			routes.MapControllerRoute("sys.getuserdata", "sys/api/getuserdata", new { controller = "Api", action = "GetUserData" });
			routes.MapControllerRoute("sys.queryuserdata", "sys/api/queryuserdata", new { controller = "Api", action = "QueryUserData" });
			routes.MapControllerRoute("sys.uiinjection", "sys/api/uiinjection", new { controller = "Api", action = "UIInjection" });

			routes.Map("sys/themes/{microService}/{theme}", (t) =>
			{
				new ThemeHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.Map("sys/globalize/{locale}/{segments}", (t) =>
			{
				new GlobalizationHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.Map("sys/bundles/{microService}/{bundle}", (t) =>
			{
				new BundleHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.Map("sys/media/{id}/{version}", (t) =>
			{
				new MediaHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.Map("sys/mail-template/{token}", (t) =>
			{
				var ve = t.RequestServices.GetService(typeof(IMailTemplateViewEngine)) as MailTemplateViewEngine;

				ve.Context = t;

				ve.Render(new Guid(t.GetRouteValue("token").ToString()));

				return Task.CompletedTask;
			});

			routes.Map("{*.}", async (t) =>
			{
				await RenderView(t);
			});

			app.Use(async (context, next) =>
			{
				if (string.Compare(context.Request.Path.Value, "/login", true) == 0)
				{
					var view = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(context.Request.Path.Value, null);

					if (view != null && view.Enabled)
					{
						await RenderView(context);
						return;
					}

				}

				await next();
			});
		}

		private static async Task RenderView(HttpContext context)
		{
			if (string.IsNullOrWhiteSpace(context.Request.Path.ToString().Trim('/')))
				context.Request.Path = "/home";

			if (Redirect(context))
			{
				await Task.CompletedTask;
				return;
			}

			var ve = context.RequestServices.GetService(typeof(IViewEngine)) as ViewEngine;

			ve.Context = context;

			ve.Render(context.Request.Path);

			await Task.CompletedTask;
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
