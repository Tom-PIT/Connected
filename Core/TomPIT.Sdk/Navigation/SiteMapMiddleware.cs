using System;
using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public abstract class SiteMapMiddleware : MiddlewareComponent, ISiteMapMiddleware
	{
		public List<ISiteMapContainer> Invoke()
		{
			return OnInvoke();
		}

		protected virtual List<ISiteMapContainer> OnInvoke()
		{
			return OnInvoke(null);
		}

		[Obsolete("Please use OnInvoke without arguments.")]
		protected virtual List<ISiteMapContainer> OnInvoke(string key)
		{
			return null;
		}

		//protected List<ISiteMapContainer> Filter(List<ISiteMapContainer> containers, string key)
		//{
		//	if (containers == null || containers.Count == 0)
		//		return containers;

		//	if (string.IsNullOrWhiteSpace(key))
		//		return containers;

		//	var result = new List<ISiteMapContainer>();

		//	foreach (var container in containers)
		//	{
		//		if (string.Compare(container.Key, key, true) == 0)
		//			result.Add(container);
		//		else
		//		{
		//			var route = Filter(container.Routes, key);
		//			container.Routes.Clear();

		//			if (route != null)
		//			{
		//				result.Add(container);
		//				container.Routes.Add(route);
		//			}
		//		}
		//	}

		//	return result;
		//}

		//private ISiteMapRoute Filter(ConnectedList<ISiteMapRoute, ISiteMapContainer> routes, string key)
		//{
		//	foreach(var route in routes)
		//	{
		//		if (string.Compare(route.RouteKey, key, true) == 0)
		//			return route;

		//		var result = Filter(route.Routes, key);

		//		if (result != null)
		//			return result;
		//	}

		//	return null;
		//}

		//private ISiteMapRoute Filter(ConnectedList<ISiteMapRoute, ISiteMapRoute> routes, string key)
		//{
		//	foreach (var route in routes)
		//	{
		//		if (string.Compare(route.RouteKey, key, true) == 0)
		//			return route;

		//		var result = Filter(route.Routes, key);

		//		if (result != null)
		//			return result;
		//	}

		//	return null;
		//}

		public List<INavigationContext> QueryContexts()
		{
			return OnQueryContexts();
		}

		protected virtual List<INavigationContext> OnQueryContexts()
		{
			return null;
		}
	}
}
