using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Proxy;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT;

public enum AuthenticationType
{
	None = 0,
	MultiTenant = 1,
	SingleTenant = 2
}

public enum InstanceState
{
	Initializing = 1,
	Running = 2
}
public static class Instance
{
	private static bool _pingRouteRegistered = false;
	private static List<IPlugin> _plugins = null;
	internal static RequestLocalizationOptions RequestLocalizationOptions { get; set; }
	public static Guid Id { get; } = Guid.NewGuid();
	public static InstanceState State { get; internal set; } = InstanceState.Initializing;
	public static CancellationToken Stopping { get; internal set; }
	public static CancellationToken Stopped { get; internal set; }
	public static InstanceFeatures Features { get; internal set; }
	public static ISysProxy SysProxy { get; internal set; }

	static Instance()
	{
		if (Shell.Configuration.RootElement.TryGetProperty("features", out JsonElement element))
			Features = Enum.Parse<InstanceFeatures>(element.GetString());
	}
	public static IStartupHostProxy Start()
	{
		return new StartupHost();
	}

	public static bool ResourceGroupExists(Guid resourceGroup)
	{
		if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
			return true;

		if (resourceGroup == Guid.Empty)
			return true;

		var resourceGroupService = MiddlewareDescriptor.Current.Tenant.GetService<IResourceGroupService>();

		var groupInstance = resourceGroupService.Select(resourceGroup);

		foreach (var i in Tenant.GetService<IResourceGroupService>().QuerySupported())
		{
			var rg = resourceGroupService.Select(i.Token);

			if (rg == groupInstance)
				return true;
		}

		return false;
	}

	public static List<IPlugin> Plugins
	{
		get
		{
			if (_plugins is null)
			{
				_plugins = new List<IPlugin>();

				foreach (var i in Runtime.Configuration.Plugins.Items)
				{
					var t = TypeExtensions.GetType(i);

					if (t is null)
						continue;

					var plugin = t.CreateInstance<IPlugin>();

					if (plugin is not null)
						_plugins.Add(plugin);
				}
			}

			return _plugins;
		}
	}

	public static void MapPingRoute(this IEndpointRouteBuilder routes)
	{
		if (_pingRouteRegistered)
			return;

		routes.Map("sys/ping", () => Results.Ok());

		//routes.MapControllerRoute("sys.ping", "sys/ping", new { controller = "Ping", action = "Invoke" });

		_pingRouteRegistered = true;
	}
}
