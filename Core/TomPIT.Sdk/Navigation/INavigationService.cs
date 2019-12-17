using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Navigation
{
	public interface INavigationService
	{
		List<string> QueryKeys();
		ISiteMapContainer QuerySiteMap(List<string> keys);
		ISiteMapContainer QuerySiteMap(List<string> keys, List<string> tags);
		List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters);
		string ParseUrl(string template, RouteValueDictionary parameters);
		ISiteMapRoute SelectRoute(string routeKey);
		ISiteMapRoute MatchRoute(string url, RouteValueDictionary parameters);
	}
}
