using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Environment;
using TomPIT.Services;

namespace TomPIT.Servers.Management
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.MultiTenant,
				ProvideApplicationParts = (args) =>
				{
					foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
					{
						var t = Types.GetType(i);

						if (t == null)
							continue;

						var template = t.CreateInstance<IMicroServiceTemplate>();

						var ds = template.GetApplicationParts();

						if (ds != null && ds.Count > 0)
							args.Parts.AddRange(ds);
					}
				}
			};

			Instance.Initialize(services, e);

			services.AddSingleton<IHostedService, InstallerService>();
		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Management, app, env, (f) =>
		  {
			  IdeRouting.Register(f.Builder, "Home", null);
		  });

			IdeBootstrapper.Run();
			ManagementBootstrapper.Run();
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
