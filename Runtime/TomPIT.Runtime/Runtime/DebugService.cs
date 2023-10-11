using System;
using System.Text.Json;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Runtime;
internal class DebugService : IDebugService
{
	public DebugService()
	{
		if (Shell.Configuration.RootElement.TryGetProperty("debugTarget", out JsonElement debugTarget))
		{
			if (debugTarget.TryGetProperty("url", out JsonElement urlNode))
				Url = urlNode.GetString();

			if (debugTarget.TryGetProperty("authenticationToken", out JsonElement tokenNode))
				AuthenticationToken = tokenNode.GetString();
		}
	}

	private string Url { get; }
	private string AuthenticationToken { get; }
	public bool Enabled => !string.IsNullOrEmpty(Url);

	public void ConfigurationAdded(Guid microService, Guid component, string category)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationAdded"), new
		{
			configuration = component
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	public void ConfigurationChanged(Guid microService, Guid component, string category)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationChanged"), new
		{
			configuration = component
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	public void ConfigurationRemoved(Guid microService, Guid component, string category)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationRemoved"), new
		{
			microService,
			configuration = component,
			category
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	public void ScriptChanged(Guid microService, Guid component, Guid element)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ScriptChanged"), new
		{
			microService,
			container = component,
			sourceCode = element
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	private string CreateUrl(string action)
	{
		return $"{Url}/NotificationDevelopment/{action}";
	}
}
