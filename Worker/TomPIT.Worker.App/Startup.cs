using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.Worker.Services;
using TomPIT.Worker.Subscriptions;

namespace TomPIT.Worker
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
				Authentication = AuthenticationType.SingleTenant
			};

			Instance.Initialize(services, e);
			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Worker, app, env, (f) =>
			{
				Worker.Configuration.Routing.Register(f.Builder);
			});

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Instance.Run(app);
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(ISubscriptionWorkerService), typeof(SubscriptionWorkerService));
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, WorkerService>();
			services.AddSingleton<IHostedService, EventService>();
			services.AddSingleton<IHostedService, SubscriptionWorker>();
			services.AddSingleton<IHostedService, SubscriptionEventWorker>();
			services.AddSingleton<IHostedService, QueueWorkerService>();
		}
	}
}
