using System.Collections.Generic;

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
	}
}
