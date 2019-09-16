using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.Environment;
using TomPIT.Navigation;
using TomPIT.Routing;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareRoutingService
	{
		string GetServer(InstanceType type, InstanceVerbs verbs);
		string ApplicationUrl(string route);
		string RestUrl(string route);
		string IoTUrl(string route);
		string CdnUrl(string route);
		string SearchUrl(string route);
		string BigDataUrl(string route);
		string Absolute(string url);
		string Resource(Guid blob);
		string Avatar(Guid user);
		string RootUrl { get; }
		bool CompareUrls(string left, string right);
		string MapPath(string relativePath);
		string RelativePath(string path);
		T RouteValue<T>(string key);
		void NotFound();
		void Forbidden();
		void Redirect(string url);
		void BadRequest();

		[Obsolete]
		string GenerateUrl(string primaryKey, string text, JArray existing, string displayProperty, string primaryKeyProperty);
		string GenerateUrl(string primaryKey, string text, Dictionary<string, string> existing);
		string GenerateUrl(string primaryKey, string text, List<IUrlRecord> existing);
		string ParseUrl(string template);
		string ParseUrl(string template, IDictionary<string, object> parameters);
		string ParseUrl(string template, RouteValueDictionary parameters);

		string ParseRoute([CAP(CAP.RouteKeysProvider)]string routeKey);
		string ParseRoute([CAP(CAP.RouteKeysProvider)]string routeKey, IDictionary<string, object> parameters);
		string ParseRoute([CAP(CAP.RouteKeysProvider)]string routeKey, RouteValueDictionary parameters);

		ISiteMapContainer QuerySiteMap([CAP(CAP.RouteSiteMapsProvider)]params string[] keys);
		ISiteMapContainer QuerySiteMap(bool authorize, [CAP(CAP.RouteSiteMapsProvider)]params string[] keys);
		List<IBreadcrumb> QueryBreadcrumbs([CAP(CAP.RouteKeysProvider)]string routeKey);
		List<IBreadcrumb> QueryBreadcrumbs([CAP(CAP.RouteKeysProvider)]string routeKey, IDictionary<string, object> parameters);
		List<IBreadcrumb> QueryBreadcrumbs([CAP(CAP.RouteKeysProvider)]string routeKey, RouteValueDictionary parameters);

		ISiteMapRoute SelectRoute([CAP(CAP.RouteKeysProvider)]string routeKey);
	}
}
