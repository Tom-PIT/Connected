using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Environment;
using TomPIT.Services;

namespace TomPIT.Servers.Development
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var parts = new List<string>();

			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				var ds = template.GetApplicationParts();

				if (ds != null)
				{
					foreach (var j in parts)
						parts.Add(j);
				}
			}

			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.MultiTenant
			};

			if (parts.Count > 0)
				e.ApplicationParts.AddRange(parts);

			Instance.Initialize(services, e);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Development, app, env, (f) =>
		  {
			  Configuration.Routing.Register(f.Builder);
			  IdeRouting.Register(f.Builder, "Ide", "ide/{microservice}");
		  });

			DevelopmentBootstrapper.Run();
			IdeBootstrapper.Run();
			Shell.GetService<IConnectivityService>().ConnectionInitializing += OnConnectionInitializing;
			Instance.Run(app);
		}

		private static void OnConnectionInitializing(object sender, SysConnectionRegisteredArgs e)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				if (template != null)
					e.Connection.GetService<IMicroServiceTemplateService>().Register(template);
			}
		}
	}
}
