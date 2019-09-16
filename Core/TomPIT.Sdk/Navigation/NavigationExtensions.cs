using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using TomPIT.Collections;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Security;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Navigation
{
	public static class NavigationExtensions
	{
		public static ISiteMapContainer WithRoutes(this ISiteMapContainer container, params ISiteMapRoute[] items)
		{
			foreach (var item in items)
				container.Routes.Add(item);

			return container;
		}

		public static ISiteMapContainer WithRoutes(this ISiteMapContainer container, List<ISiteMapRoute> items)
		{
			if (items == null)
				return container;

			foreach (var item in items)
				container.Routes.Add(item);

			return container;
		}

		public static ISiteMapRoute WithRoutes(this ISiteMapRoute route, params ISiteMapRoute[] items)
		{
			foreach (var item in items)
				route.Routes.Add(item);

			return route;
		}

		public static ISiteMapRoute WithRoutes(this ISiteMapRoute route, List<ISiteMapRoute> items)
		{
			if (items == null)
				return route;

			foreach (var item in items)
				route.Routes.Add(item);

			return route;
		}

		public static ISiteMapContainer Container(this ISiteMapRoute item)
		{
			var current = item as ISiteMapElement;

			while (current.Parent != null)
				current = current.Parent;

			return current as ISiteMapContainer;
		}

		public static string ParseUrl(this ISiteMapRoute link, IMiddlewareContext context)
		{
			if (string.IsNullOrWhiteSpace(link.Template))
				return null;

			var routeData = Shell.HttpContext == null ? new RouteData() : Shell.HttpContext.GetRouteData();

			return context.Services.Routing.ParseUrl(link.Template, routeData.Values);
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IMiddlewareContext context, [CAP(CAP.RouteKeysProvider)]string routeKey, Dictionary<string, object> parameters)
		{
			var breadcrumbs = context.Services.Routing.QueryBreadcrumbs(routeKey, parameters);

			if (breadcrumbs == null)
				return;

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IMiddlewareContext context, [CAP(CAP.RouteKeysProvider)]string routeKey, RouteValueDictionary parameters)
		{
			var breadcrumbs = context.Services.Routing.QueryBreadcrumbs(routeKey, parameters);

			if (breadcrumbs == null)
				return;

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IMiddlewareContext context, [CAP(CAP.RouteKeysProvider)]string routeKey)
		{
			var breadcrumbs = context.Services.Routing.QueryBreadcrumbs(routeKey);

			if (breadcrumbs == null)
				return;

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}

		}

		public static void FromSiteMap(this List<IRoute> routes, IMiddlewareContext context, [CAP(CAP.RouteSiteMapsProvider)]string routeKey)
		{
			var sitemap = context.Services.Routing.QuerySiteMap(routeKey);

			if (sitemap == null)
				return;

			LoadRoutes(context, routes, sitemap.Routes);
		}

		private static void LoadRoutes(IMiddlewareContext context, List<IRoute> routes, ConnectedList<ISiteMapRoute, ISiteMapContainer> items)
		{
			foreach (var route in items)
			{
				var url = new Route
				{
					Text = route.Text,
					Url = route.ParseUrl(context)
				};

				routes.Add(url);

				LoadRoutes(context, url.Items, route.Routes);
			}
		}

		private static void LoadRoutes(IMiddlewareContext context, List<IRoute> routes, ConnectedList<ISiteMapRoute, ISiteMapRoute> items)
		{
			foreach (var route in items)
			{
				var url = new Route
				{
					Text = route.Text,
					Url = route.ParseUrl(context)
				};

				routes.Add(url);

				LoadRoutes(context, url.Items, route.Routes);
			}
		}

		public static ISiteMapContainer WithAuthorization(this ISiteMapContainer container, IMiddlewareContext context)
		{
			context.Tenant.GetService<IAuthorizationService>().Authorize(container);

			return container;
		}

		public static string RouteUrl(this MiddlewareContext context, string routeName, object values)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrCannotResolveHttpRequest);

			var svc = Shell.HttpContext.RequestServices.GetService(typeof(IUrlHelperFactory)) as IUrlHelperFactory;

			if (svc == null)
				throw new RuntimeException(SR.ErrNoUrlHelper);

			/*
			 * It's IRequestContextProvider for sure otherwise request would be null
			 */
			var ac = context as IActionContextProvider;

			var helper = svc.GetUrlHelper(ac.ActionContext);

			return helper.RouteUrl(routeName, values);
		}
	}
}
