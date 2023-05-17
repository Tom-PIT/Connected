using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TomPIT.App;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.Sys;

namespace TomPIT.Connected
{
	internal class Startup
	{
		public Startup()
		{
			Startups = new();

			Instance.Boot();

			if (Instance.Features.HasFlag(InstanceFeatures.Sys))
				SysStartup = new SysStartup();

			if (Instance.Features.HasFlag(InstanceFeatures.Application))
				Startups.Add(new AppStartup());
		}

		private SysStartup? SysStartup { get; }
		private List<IInstanceStartup> Startups { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			if (SysStartup is not null)
				SysStartup.ConfigureServices(services);

			Instance.Initialize();

			foreach (var startup in Startups)
				startup.Initialize();

			Instance.InitializeTenant();
			Instance.InitializeServices(services, new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.SingleTenant,
				CorsEnabled = true
			});

			foreach (var startup in Startups)
				startup.ConfigureServices(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (SysStartup is not null)
				SysStartup.Configure(app, env);

			foreach (var startup in Startups)
				startup.Configure(app, env);

			Instance.Configure(app, env,
			(f) =>
			{
				foreach (var startup in Startups)
					startup.ConfigureRouting(f);
			},
			(f) =>
			{
				foreach (var startup in Startups)
					startup.ConfigureMiddleware(f);
			});

			Instance.Run(app, env);
		}
	}
}
