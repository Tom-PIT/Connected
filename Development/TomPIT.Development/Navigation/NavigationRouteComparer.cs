using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Development.Navigation
{
	internal class NavigationRouteComparer : EqualityComparer<INavigationRouteDescriptor>
	{
		public override bool Equals(INavigationRouteDescriptor x, INavigationRouteDescriptor y)
		{
			if (x is null || y is null)
				return false;

			if (string.Compare(x.RouteKey, y.RouteKey, true) != 0)
				return false;

			if (string.Compare(x.Template, y.Template, true) != 0)
				return false;

			return true;
		}

		public override int GetHashCode([DisallowNull] INavigationRouteDescriptor obj)
		{
			return obj.GetHashCode();
		}
	}
}
