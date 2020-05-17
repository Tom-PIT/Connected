using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using TomPIT.Environment;
using TomPIT.Navigation;
using TomPIT.Routing;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

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

		string GenerateUrl(string primaryKey, string text, Dictionary<string, string> existing);
		string GenerateUrl(string primaryKey, string text, List<IUrlRecord> existing);
		string ParseUrl(string template);
		string ParseUrl(string template, IDictionary<string, object> parameters);
		string ParseUrl(string template, RouteValueDictionary parameters);

		string ParseRoute([CIP(CIP.RouteKeyProvider)]string routeKey);
		string ParseRoute([CIP(CIP.RouteKeyProvider)]string routeKey, object parameters);
		string ParseRoute([CIP(CIP.RouteKeyProvider)]string routeKey, RouteValueDictionary parameters);

		ISiteMapContainer QuerySiteMap(List<string> keys);
		ISiteMapContainer QuerySiteMap(List<string> keys, bool authorize);
		ISiteMapContainer QuerySiteMap(List<string> keys, bool authorize, List<string> tags);
		List<IBreadcrumb> QueryBreadcrumbs([CIP(CIP.RouteKeyProvider)]string routeKey);
		List<IBreadcrumb> QueryBreadcrumbs([CIP(CIP.RouteKeyProvider)]string routeKey, object parameters);
		List<IBreadcrumb> QueryBreadcrumbs([CIP(CIP.RouteKeyProvider)]string routeKey, RouteValueDictionary parameters);

		ISiteMapRoute SelectRoute([CIP(CIP.RouteKeyProvider)]string routeKey);
	}
}
