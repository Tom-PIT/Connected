using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.Environment;
using TomPIT.Navigation;
using TomPIT.Routing;

namespace TomPIT.Services.Context
{
	public interface IContextRoutingService
	{
		string GetServer(InstanceType type, InstanceVerbs verbs);
		string ApplicationUrl(string route);
		string RestUrl(string route);
		string IoTUrl(string route);
		string CdnUrl(string route);
		string SearchUrl(string route);
		string BigDataUrl(string route);
		string Absolute(string url);
		string Resource(IUrlHelper helper, Guid blob);
		string Avatar(Guid user);

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

		ISiteMapContainer QuerySiteMap(params string[] keys);
		List<IBreadcrumb> QueryBreadcrumbs([CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey);
		List<IBreadcrumb> QueryBreadcrumbs([CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey, IDictionary<string, object> parameters);
		List<IBreadcrumb> QueryBreadcrumbs([CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey, RouteValueDictionary parameters);

		ISiteMapRoute SelectLink([CodeAnalysisProvider(CodeAnalysisProviderAttribute.RouteKeysProvider)]string routeKey);
	}
}
