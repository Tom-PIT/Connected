using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Management.Deployment;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Management
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
						var t = Reflection.TypeExtensions.GetType(i);

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

			services.AddHostedService<InstallerService>();
			services.AddHostedService<UpdateService>();
			services.AddHostedService<PublishService>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(InstanceType.Management, app, env, (f) =>
		 {
			 IdeRouting.Register(f.Builder, "Home", string.Empty);
		 });

			IdeBootstrapper.Run();
			ManagementBootstrapper.Run();
			Shell.GetService<IConnectivityService>().TenantInitialized += OnTenantInitialized;
			Instance.Run(app);
		}

		private static void OnTenantInitialized(object sender, TenantArgs e)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Reflection.TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				if (template != null)
					e.Tenant.GetService<IMicroServiceTemplateService>().Register(template);
			}
		}
	}
}
