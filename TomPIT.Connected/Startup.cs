using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TomPIT.App;
using TomPIT.BigData;
using TomPIT.Cdn;
using TomPIT.Development;
using TomPIT.Environment;
using TomPIT.IoT;
using TomPIT.Management;
using TomPIT.Rest;
using TomPIT.Runtime;
using TomPIT.Search;
using TomPIT.Startup;
using TomPIT.Sys;
using TomPIT.Worker;

namespace TomPIT.Connected;

internal class Startup
{
	public Startup()
	{
		var startups = new List<IStartupClient>();

		if (Instance.Features.HasFlag(InstanceFeatures.Sys))
			startups.Add(new SysStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Development))
			startups.Add(new DevStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.BigData))
			startups.Add(new BigDataStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Cdn))
			startups.Add(new CdnStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Management))
			startups.Add(new ManagementStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Rest))
			startups.Add(new RestStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Search))
			startups.Add(new SearchStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Worker))
			startups.Add(new WorkerStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.IoT))
			startups.Add(new IoTStartup());

		if (Instance.Features.HasFlag(InstanceFeatures.Application))
			startups.Add(new AppStartup());

		Host = Instance.Start();

		Host.ConfiguringServices += OnConfiguringServices;

		foreach (var startup in startups)
			startup.Initialize(Host);
	}

	private void OnConfiguringServices(object? sender, IServiceCollection e)
	{
		if (Tenant.GetService<IMicroServiceRuntimeService>() is IMicroServiceRuntimeService runtimeService)
			runtimeService.Configure(e);
	}

	private IStartupHostProxy Host { get; }

	public void ConfigureServices(IServiceCollection services)
	{
		Host.ConfigureServices(services);
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		Host.Configure(app, env);

		Design.BaseMicroServiceInstaller.InitializeInstance();
	}
}
