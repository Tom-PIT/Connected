using System;
using System.Collections.Generic;
using System.Linq;
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
using TomPIT.Runtime;
using TomPIT.UI;

namespace TomPIT.App.Routing
{
	internal static class AppRouting
	{
		public static void RegisterRouteMiddleware(IApplicationBuilder app) 
		{
			app.Use(async (context, next) =>
			{
				if (string.Compare(context.Request.Path.Value, "/login", true) == 0)
				{
					var view = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(context.Request.Path.Value, null);

					if (view?.Enabled ?? false)
					{
						await RenderView(context);
						return;
					}

				}
                if (next is not null && !context.Response.HasStarted)
                    await next();
			});
		}
		public static void Register(IEndpointRouteBuilder routes)
		{
			routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });
			routes.MapControllerRoute("sys.api", "sys/api/invoke", new { controller = "Api", action = "Invoke" });
			routes.MapControllerRoute("sys.search", "sys/api/search", new { controller = "Api", action = "Search" });
			routes.MapControllerRoute("sys.partial", "sys/api/partial", new { controller = "Api", action = "Partial" });
			routes.MapControllerRoute("sys.setuserdata", "sys/api/setuserdata", new { controller = "Api", action = "SetUserData" });
			routes.MapControllerRoute("sys.getuserdata", "sys/api/getuserdata", new { controller = "Api", action = "GetUserData" });
			routes.MapControllerRoute("sys.queryuserdata", "sys/api/queryuserdata", new { controller = "Api", action = "QueryUserData" });
			routes.MapControllerRoute("sys.uiinjection", "sys/api/uiinjection", new { controller = "Api", action = "UIInjection" });
			routes.MapControllerRoute("sys.tracing.endpoints", "sys/tracing/endpoints", new { controller = "Tracing", action = "Endpoints" });

			routes.Map("sys/themes/{microService}/{theme}", (t) =>
			{
				new ThemeHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});

			routes.Map("static/{microService}/{*path}", (t) =>
			{
				new StaticHandler().ProcessRequest(t);

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

			routes.Map("sys/mail-template/{token}", async (t) =>
			{
				var ve = t.RequestServices.GetService(typeof(IMailTemplateViewEngine)) as MailTemplateViewEngine;

				ve.Context = t;

				await ve.Render(new Guid(t.GetRouteValue("token").ToString()));
			});

			var builder = routes.Map("{*.}", async (t) =>
			{
				await RenderView(t);
			});

			builder.Add(b => ((RouteEndpointBuilder)b).Order = int.MaxValue);			
		}

		private static async Task RenderView(HttpContext context)
		{
			if (string.IsNullOrWhiteSpace(context.Request.Path.ToString().Trim('/')))
				context.Request.Path = "/home";

			if (Redirect(context))
				return;
			else if (Download(context))
				return;

			var ve = context.RequestServices.GetService(typeof(IViewEngine)) as ViewEngine;

			ve.Context = context;

			await ve.Render(context.Request.Path);
		}

		private static bool Redirect(HttpContext context)
		{
			if (context.Request.Path.ToString().StartsWith("/home"))
			{
				if (HomeResolved(context))
					return true;
			}

			var routes = new RouteValueDictionary();

			var route = MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(context.Request.Path, routes);

			if (route is ISiteMapRedirectRoute redirect)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Redirect;
				context.Response.Redirect(redirect.RedirectUrl(routes));

				return true;
			}

			return false;
		}

		private static bool Download(HttpContext context)
		{
			var routes = new RouteValueDictionary();

			var route = MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(context.Request.Path, routes);
			
			if (route is ISiteMapStreamRoute stream)
			{
				using var ctx = new MiddlewareContext(MiddlewareDescriptor.Current.Tenant.Url);

				ctx.Interop.Invoke(stream.Api, stream.Parameters);

				return true;
			}

			return false;
		}

		private static bool HomeResolved(HttpContext context)
		{
			var runtimes = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceRuntimeService>().QueryRuntimes();
			var resolvedHomeUrls = new List<IRuntimeUrl>();

			foreach (var middleware in runtimes)
			{
				var homeUrl = middleware.ResolveUrl(RuntimeUrlKind.Default);

				if (homeUrl != null)
					resolvedHomeUrls.Add(homeUrl);
			}

			if (resolvedHomeUrls.Count == 0)
				return false;

			var winner = resolvedHomeUrls.OrderByDescending(f => f.Weight).First();

			context.Response.StatusCode = (int)HttpStatusCode.Redirect;
			context.Response.Redirect(winner.Url);

			return true;
		}
	}
}
