using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.IoT.Hubs;
using TomPIT.IoT.Security;
using TomPIT.IoT.Services;
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
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
			Instance.Run(app);

			app.UseSignalR(routes =>
			{
				routes.MapHub<IoTHub>("/iot");
			});

			Instance.Connection.GetService<IAuthorizationService>().RegisterAuthenticationProvider(new IoTAuthenticationProvider());
		}

		private void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(IIoTHubService), typeof(IoTHubService));
		}
	}
}
