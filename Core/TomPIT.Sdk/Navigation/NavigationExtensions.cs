using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Security;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;
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
			var url = context.Services.Routing.ParseUrl(link.Template, routeData.Values.Merge(link.Parameters));

			if (!string.IsNullOrWhiteSpace(link.QueryString))
				url = $"{url}?{link.QueryString}";

			return link.WithNavigationContext(url);
		}

		public static string WithNavigationContext(this ISiteMapRouteContainer route, string parsedUrl)
		{
			if (string.IsNullOrWhiteSpace(parsedUrl))
				return parsedUrl;

			return WithNavigationContext(route.NavigationContext, parsedUrl);
		}

		public static string WithNavigationContext(this ISiteMapRoute route, string parsedUrl)
		{
			if (string.IsNullOrWhiteSpace(parsedUrl))
				return parsedUrl;

			return WithNavigationContext(route.NavigationContext, parsedUrl);
		}

		private static string WithNavigationContext(string key, string parsedUrl)
		{
			if (Shell.HttpContext?.Request.Query.ContainsKey("navigationContext") == true)
				key = Shell.HttpContext?.Request.Query["navigationContext"];

			if (string.IsNullOrWhiteSpace(key))
				return parsedUrl;

			if (parsedUrl.Contains("?"))
			{
				var tokens = parsedUrl.Split("?".ToCharArray(), 2);

				if (tokens.Length > 1)
				{
					if (QueryHelpers.ParseQuery(tokens[1]).ContainsKey("navigationContext"))
						return parsedUrl;
				}

				return $"{parsedUrl}&navigationContext={key}";
			}
			else
				return $"{parsedUrl}?navigationContext={key}";
		}

		public static RouteValueDictionary Merge(this RouteValueDictionary existing, ISiteMapElement element)
		{
			if (element is ISiteMapRoute route)
				return existing.Merge(route.Parameters);
			else if (element is ISiteMapRouteContainer container)
				return existing.Merge(container.Parameters);

			return existing;
		}
		public static RouteValueDictionary Merge(this RouteValueDictionary existing, object parameters)
		{
			var result = new RouteValueDictionary(existing);

			if (parameters == null)
				return result;

			var props = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in props)
			{
				if (result.ContainsKey(prop.Name))
					result[prop.Name] = prop.GetValue(parameters);
				else
					result.Add(prop.Name, prop.GetValue(parameters));
			}

			return result;
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IMiddlewareContext context, [CIP(CIP.RouteKeyProvider)][AA(AA.RouteKeyAnalyzer)] string routeKey, Dictionary<string, object> parameters)
		{
			FromBreadcrumbs(routes, context, routeKey, parameters == null ? null as RouteValueDictionary : new RouteValueDictionary(parameters));
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IMiddlewareContext context, [CIP(CIP.RouteKeyProvider)][AA(AA.RouteKeyAnalyzer)] string routeKey, RouteValueDictionary parameters)
		{
			if (context.Tenant.GetService<INavigationService>().QueryBreadcrumbs(routeKey, parameters, routes.Any() ? BreadcrumbLinkBehavior.All : BreadcrumbLinkBehavior.IgnoreLastRoute) is not List<IBreadcrumb> breadcrumbs)
				return;

			var existing = routes.ToImmutableArray();

			routes.Clear();

			foreach (var breadcrumb in breadcrumbs)
			{
				routes.Add(new Route
				{
					Text = breadcrumb.Text,
					Url = breadcrumb.Url
				});
			}

			foreach (var route in existing)
				routes.Add(route);
		}

		public static void FromBreadcrumbs(this List<IRoute> routes, IMiddlewareContext context, [CIP(CIP.RouteKeyProvider)][AA(AA.RouteKeyAnalyzer)] string routeKey)
		{
			FromBreadcrumbs(routes, context, routeKey, null as RouteValueDictionary);
		}

		public static void FromSiteMap(this List<IRoute> routes, IMiddlewareContext context, [CIP(CIP.RouteSiteMapsProvider)][AA(AA.RouteKeyAnalyzer)] string routeKey)
		{
			var sitemap = context.Services.Routing.QuerySiteMap(new List<string> { routeKey });

			if (sitemap == null)
				return;

			LoadRoutes(context, routes, sitemap.Routes);
		}

		public static void FromSiteMap(this List<IRoute> routes, IMiddlewareContext context, [CIP(CIP.RouteSiteMapsProvider)][AA(AA.RouteKeyAnalyzer)] string routeKey, string tag)
		{
			var sitemap = context.Services.Routing.QuerySiteMap(new List<string> { routeKey }, true, new List<string> { tag });

			if (sitemap == null)
				return;

			LoadRoutes(context, routes, sitemap.Routes);
		}

		private static void LoadRoutes(IMiddlewareContext context, List<IRoute> routes, ConnectedList<ISiteMapRoute, ISiteMapContainer> items)
		{
			foreach (var route in items)
			{
				var url = CreateRoute(context, route);

				routes.Add(url);

				LoadRoutes(context, url.Items, route.Routes);
			}
		}

		private static void LoadRoutes(IMiddlewareContext context, List<IRoute> routes, ConnectedList<ISiteMapRoute, ISiteMapRoute> items)
		{
			foreach (var route in items)
			{
				var url = CreateRoute(context, route);
				
				routes.Add(url);

				LoadRoutes(context, url.Items, route.Routes);
			}
		}

		private static Route CreateRoute(IMiddlewareContext context, ISiteMapRoute route)
		{
			return new Route
			{
				Text = route.Text,
				Url = route.ParseUrl(context),
				BeginGroup = route.BeginGroup,
				Visible = route.Visible,
				Glyph = route.Glyph,
				Css = route.Css,
				Category = route.Category,
				Ordinal = route.Priority,
				Priority = route.Priority,
				Id = route.RouteKey
			};
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
			 * It's the IActionContextProvider for sure otherwise request would be null
			 */
			var ac = context as IActionContextProvider;

			var helper = svc.GetUrlHelper(ac.ActionContext);

			return helper.RouteUrl(routeName, values);
		}

		internal static string ResolveRouteTemplate(IMiddlewareContext context, string viewName)
		{
			if (string.IsNullOrWhiteSpace(viewName) || context == null)
				return null;

			var view = ComponentDescriptor.View(context, viewName);

			if (view.Configuration == null)
				return null;

			return view.Configuration.Url;
		}


		/// <summary>
		/// Searches <see cref="ISiteMapContainer"/> for route with specified key.
		/// </summary>
		/// <param name="container">Instance of <see cref="ISiteMapContainer"/>.</param>
		/// <param name="routeKey">Route key.</param>
		/// <returns>Route (with child routes) if found; <code>null</code> otherwise.</returns>
		public static ISiteMapRoute FindSiteMapRoute(this ISiteMapContainer container, string routeKey)
		{
			if (container == null || container.Routes == null || container.Routes.Count < 1)
			{
				return null;
			}

			foreach (var r in container.Routes)
			{
				var retVal = r.FindSiteMapRoute(routeKey);
				if (retVal != null)
				{
					return retVal;
				}
			}

			return null;
		}

		/// <summary>
		/// Searches <see cref="ISiteMapRoute"/> for route with specified key.
		/// </summary>
		/// <param name="route">Instance of <see cref="ISiteMapRoute"/>.</param>
		/// <param name="routeKey">Route key.</param>
		/// <returns>Route (with child routes) if found; <code>null</code> otherwise.</returns>
		public static ISiteMapRoute FindSiteMapRoute(this ISiteMapRoute route, string routeKey)
		{
			if (route == null)
			{
				return null;
			}

			if (string.Compare(routeKey, route.RouteKey, true) == 0)
			{
				return route;
			}

			if (route.Routes == null || route.Routes.Count < 1)
			{
				return null;
			}

			foreach (var r in route.Routes)
			{
				var retVal = r.FindSiteMapRoute(routeKey);
				if (retVal != null)
				{
					return retVal;
				}
			}

			return null;
		}
	}
}
