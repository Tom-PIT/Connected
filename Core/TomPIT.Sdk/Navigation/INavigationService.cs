using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Navigation
{
	public interface INavigationService
	{
		ISiteMapContainer QuerySiteMap(params string[] keys);
		List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters);
		string ParseUrl(string template, RouteValueDictionary parameters);
		ISiteMapRoute SelectRoute(string routeKey);
	}
}
