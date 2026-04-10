using Microsoft.AspNetCore.Routing;
using System;
using System.Net;
using TomPIT.Design;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Routing;

internal class DebugRouteHandler : RouteHandlerBase
{
	protected override void OnProcessRequest()
	{
		var ctx = Tenant ?? MiddlewareDescriptor.Current.Tenant;

		if (!ctx.GetService<IAuthorizationService>().Demand(MiddlewareDescriptor.Current.UserToken, SecurityUtils.FullControlRole))
		{
			Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			return;
		}

		var proxy = Tenant.GetService<IDesignNotificationProxy>();
		var action = Context.GetRouteValue("action") as string;

		if (string.Equals(action, "ConfigurationChanged", StringComparison.OrdinalIgnoreCase))
		{
			var component = Context.Request.Body.ToJObject().Required<Guid>("component");
			proxy.ConfigurationChanged(component);
		}
		else if (string.Equals(action, "ConfigurationRemoved", StringComparison.OrdinalIgnoreCase))
		{
			var component = Context.Request.Body.ToJObject().Required<Guid>("component");
			proxy.ConfigurationRemoved(component);
		}
		else if (string.Equals(action, "ConfigurationAdded", StringComparison.OrdinalIgnoreCase))
		{
			var component = Context.Request.Body.ToJObject().Required<Guid>("component");
			proxy.ConfigurationAdded(component);
		}
		else if (string.Equals(action, "SourceTextChanged", StringComparison.OrdinalIgnoreCase))
		{
			var body = Context.Request.Body.ToJObject();
			proxy.SourceTextChanged(
				body.Required<Guid>("microService"),
				body.Required<Guid>("component"),
				body.Required<Guid>("token"),
				body.Required<int>("type"));
		}
	}
}

