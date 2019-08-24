using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Development, app, env, (f) =>
		  {
			  RegisterDesignersRouting(f.Builder);

			  Configuration.Routing.Register(f.Builder);
			  IdeRouting.Register(f.Builder, "Ide", "ide/{microservice}");
		  });

			DevelopmentBootstrapper.Run();
			IdeBootstrapper.Run();
			Shell.GetService<IConnectivityService>().ConnectionInitialized += OnConnectionInitialized;
			Instance.Run(app);

			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				template.Initialize(app, env);
			}
		}

		private void RegisterDesignersRouting(IRouteBuilder builder)
		{
			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				template.RegisterRoutes(builder);
			}
		}

		private static void OnConnectionInitialized(object sender, SysConnectionArgs e)
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
