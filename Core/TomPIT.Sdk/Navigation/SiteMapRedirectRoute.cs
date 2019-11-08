using Microsoft.AspNetCore.Routing;

namespace TomPIT.Navigation
{
	public abstract class SiteMapRedirectRoute : SiteMapRoute, ISiteMapRedirectRoute
	{
		public string RedirectUrl(RouteValueDictionary routes)
		{
			return OnRedirectUrl(routes);
		}

		protected abstract string OnRedirectUrl(RouteValueDictionary routes);
	}
}
