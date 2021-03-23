using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.IoT.Hubs;
using TomPIT.IoT.Routing;
using TomPIT.IoT.Services;
using TomPIT.Runtime;

namespace TomPIT.IoT
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.SingleTenant,
				CorsEnabled = true
			};

			Instance.Initialize(services, e);

			services.AddSignalR((o) =>
			{
				o.EnableDetailedErrors = true;
			}).AddNewtonsoftJsonProtocol();

			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(InstanceType.IoT, app, env, (f) =>
			{
				IoTRouting.Register(f.Builder);
				f.Builder.MapHub<IoTServerHub>("/iot");
			});

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Instance.Run(app, env);
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IIoTHubService), typeof(IoTHubService));
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, FlushingService>();
		}
	}
}
