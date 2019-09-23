using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.IoT.Hubs;
using TomPIT.IoT.Routing;
using TomPIT.IoT.Security;
using TomPIT.IoT.Services;
using TomPIT.Runtime;
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
					var setting = Instance.Tenant.GetService<ISettingService>().Select(Guid.Empty, "Cors Origins");
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
				IoTRouting.Register(f.Builder);
			});

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Instance.Run(app);

			app.UseCors("TomPITPolicy");
			app.UseSignalR(routes =>
			{
				routes.MapHub<IoTServerHub>("/iot");
			});

			Instance.Tenant.GetService<IAuthorizationService>().RegisterAuthenticationProvider(new IoTAuthenticationProvider());
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IIoTHubService), typeof(IoTHubService));
		}
	}
}
