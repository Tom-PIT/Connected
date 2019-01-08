using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Application;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Environment;
using TomPIT.IoT;

namespace TomPIT.Servers.Development
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.Jwt
			};

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
			e.Connection.GetService<IMicroServiceTemplateService>().Register(new ApplicationTemplate());
			e.Connection.GetService<IMicroServiceTemplateService>().Register(new IoTTemplate());
		}
	}
}
