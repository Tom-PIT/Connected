using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TomPIT.App;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Connected
{
	internal class Startup
	{
		public Startup()
		{
			Startups = new();

			Instance.Boot();
			Features = Shell.GetConfiguration<ISys>().Features;

			if (Features.HasFlag(InstanceFeatures.Application))
				Startups.Add(new AppStartup());
		}

		private List<IInstanceStartup> Startups { get; }
		private InstanceFeatures Features { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			Instance.Initialize(Features, services, new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.SingleTenant,
				CorsEnabled = true
			});

			foreach (var startup in Startups)
				startup.Initialize();

			Instance.InitializeShellServices();

			foreach (var startup in Startups)
				startup.ConfigureServices(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
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
