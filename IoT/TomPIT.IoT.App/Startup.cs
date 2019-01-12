using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Environment;
using TomPIT.IoT.Hubs;
using TomPIT.IoT.Security;
using TomPIT.Security;

namespace TomPIT.IoT
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.Bearer
			};

			Instance.Initialize(services, e);
			services.AddSignalR();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Rest, app, env, null);
			Instance.Run(app);

			app.UseSignalR(routes =>
			{
				routes.MapHub<IoTHub>("/iot");
			});

			Instance.Connection.GetService<IAuthorizationService>().RegisterAuthenticationProvider(new IoTAuthenticationProvider());
		}
	}
}
