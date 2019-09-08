using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TomPIT.Configuration;
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
				Authentication = AuthenticationType.SingleTenant
			};

			Instance.Initialize(services, e);

			services.AddCors(options => options.AddPolicy("TomPITPolicy",
				builder =>
				{
					var setting = Instance.GetService<ISettingService>().Select(Guid.Empty, "Cors Origins");
					var origin = new string[] { "http://localhost" };

					if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
						origin = setting.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

					builder.AllowAnyMethod()
					.AllowAnyHeader()
					.WithOrigins(origin)
					.AllowCredentials();
				}));

			services.AddSignalR((o) =>
			{
				o.EnableDetailedErrors = true;
			});

			services.AddSingleton<IHostedService, FlushingService>();
		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Rest, app, env, (f) =>
			{
				Configuration.Routing.Register(f.Builder);
			});

			Shell.GetService<IConnectivityService>().ConnectionInitialize += OnConnectionInitialize;
			Instance.Run(app);

			app.UseCors("TomPITPolicy");
			app.UseSignalR(routes =>
			{
				routes.MapHub<IoTServerHub>("/iot");
			});

			Instance.Connection.GetService<IAuthorizationService>().RegisterAuthenticationProvider(new IoTAuthenticationProvider());
		}

		private void OnConnectionInitialize(object sender, SysConnectionArgs e)
		{
			e.Connection.RegisterService(typeof(IIoTHubService), typeof(IoTHubService));
		}
	}
}
