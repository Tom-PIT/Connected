using System.Collections.Generic;

namespace TomPIT.Navigation
{
	public static class NavigationExtensions
	{
		public static ISiteMapContainer WithItems(this ISiteMapContainer container, params ISiteMapRoute[] items)
		{
			foreach (var item in items)
				container.Items.Add(item);

			return container;
		}

		public static ISiteMapContainer WithItems(this ISiteMapContainer container, List<ISiteMapRoute> items)
		{
			if (items == null)
				return container;

			foreach (var item in items)
				container.Items.Add(item);

			return container;
		}

		public static ISiteMapRoute WithItems(this ISiteMapRoute route, params ISiteMapRoute[] items)
		{
			foreach (var item in items)
				route.Items.Add(item);

			return route;
		}

		public static ISiteMapRoute WithItems(this ISiteMapRoute route, List<ISiteMapRoute> items)
		{
			if (items == null)
				return route;

			foreach (var item in items)
				route.Items.Add(item);

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
