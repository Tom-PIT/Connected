﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
					builder.AllowAnyMethod()
					.AllowAnyHeader()
					.WithOrigins("http://localhost:44003")
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
			Instance.Configure(InstanceType.Rest, app, env, null);
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
			Instance.Run(app);

			app.UseCors("TomPITPolicy");
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