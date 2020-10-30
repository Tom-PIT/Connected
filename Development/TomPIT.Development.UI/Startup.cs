using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Connectivity;
using TomPIT.Development.Analysis;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Development
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
						var t = TypeExtensions.GetType(i);

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

			services.AddHostedService<ToolsRunner>();
			services.AddHostedService<AutoFixRunner>();
			services.AddHostedService<ComponentAnalysisRunner>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(InstanceType.Development, app, env, (f) =>
		  {
			  RegisterDesignersRouting(f.Builder);

			  Configuration.Routing.Register(f.Builder);
			  IdeRouting.Register(f.Builder, "Ide", "ide/{microservice}");
		  });

			DevelopmentBootstrapper.Run();
			IdeBootstrapper.Run();

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Shell.GetService<IConnectivityService>().TenantInitialized += OnTenantInitialized;

			Instance.Run(app);

			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				template.Initialize(app, env);
			}
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IAutoFixService), typeof(AutoFixService));
		}

		private void RegisterDesignersRouting(IEndpointRouteBuilder builder)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				template.RegisterRoutes(builder);
			}
		}

		private static void OnTenantInitialized(object sender, TenantArgs e)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				if (template != null)
					e.Tenant.GetService<IMicroServiceTemplateService>().Register(template);
			}
		}
	}
}
