using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Environment;

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

		void NotFound();
		void Forbidden();
		void Redirect(string url);
		void BadRequest();

		string GenerateUrl(string primaryKey, string text, JArray existing, string displayProperty, string primaryKeyProperty);
		string ParseUrl(string template, IDictionary<string, object> parameters);
	}
}
