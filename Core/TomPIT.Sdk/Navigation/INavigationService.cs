using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Navigation
{
	public interface INavigationService
	{
		List<string> QueryKeys();
		ISiteMapContainer QuerySiteMap(List<string> keys);
		List<ISiteMapContainer> QueryContainers();
		ISiteMapContainer QuerySiteMap(List<string> keys, List<string> tags);
		List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters);
		string ParseUrl(string template, RouteValueDictionary parameters);
		string ParseUrl(string template, RouteValueDictionary parameters, bool allowQueryString);
		ISiteMapRoute SelectRoute(string routeKey);
		ISiteMapRoute MatchRoute(string url, RouteValueDictionary parameters);
	}
}
