using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Navigation
{
	public enum BreadcrumbLinkBehavior
	{
		All = 1,
		IgnoreLastRoute = 2
	}

	public interface INavigationService
	{
		List<string> QueryKeys();
		ISiteMapContainer QuerySiteMap(List<string> keys);
		List<ISiteMapContainer> QueryContainers();
		ISiteMapContainer QuerySiteMap(List<string> keys, List<string> tags);
		List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters);
		List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters, BreadcrumbLinkBehavior linkBehavior);
		string ParseUrl(string template, RouteValueDictionary parameters);
		string ParseUrl(string template, RouteValueDictionary parameters, bool allowQueryString);
		ISiteMapRoute SelectRoute(string routeKey);
		ISiteMapRoute MatchRoute(string url, RouteValueDictionary parameters);
		INavigationContext SelectNavigationContext(string key);
	}
}
