using System;
using System.Text;
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

			if (debugTarget.TryGetProperty("token", out JsonElement tokenNode))
				AuthenticationToken = Encoding.UTF8.GetString(Convert.FromBase64String(tokenNode.GetString()));
		}
	}

	private string Url { get; }
	private string AuthenticationToken { get; }
	public bool Enabled => !string.IsNullOrEmpty(Url);

	public void ConfigurationAdded(Guid component)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationAdded"), new
		{
			component
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	public void ConfigurationChanged(Guid component)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationChanged"), new
		{
			component
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	public void ConfigurationRemoved(Guid component)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ConfigurationRemoved"), new
		{
			component
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	public void ScriptChanged(Guid microService, Guid component, Guid element, Guid token)
	{
		if (!Enabled)
			return;

		MiddlewareDescriptor.Current.Tenant.Post(CreateUrl("ScriptChanged"), new
		{
			microService,
			component,
			element,
			token
		}, new HttpRequestArgs().WithBearerCredentials(AuthenticationToken));
	}

	private string CreateUrl(string action)
	{
		return $"{Url}/sys/debug/{action}";
	}
}
