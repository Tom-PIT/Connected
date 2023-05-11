using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Diagnostics;
using System.Linq;

using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.HealthMonitoring;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;
using TomPIT.Worker.HostedServices;
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

			Instance.Initialize(InstanceFeatures.Worker, services, e);

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;

			Instance.InitializeShellServices();

			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(app, env, (f) =>
			{
				Worker.Configuration.Routing.Register(f.Builder);
			});

			Instance.Run(app, env);

			Tenant.GetService<IHostedServices>().Initialize();

			app.ApplicationServices.GetService<IQueueMonitoringService>();
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(ISubscriptionWorkerService), typeof(SubscriptionWorkerService));
			e.Tenant.RegisterService(typeof(IWorkerProxyService), typeof(WorkerProxyService));
			e.Tenant.RegisterService(typeof(IHostedServices), typeof(HostedServicesContainer));
			e.Tenant.RegisterService(typeof(IHealthMonitoringService), typeof(HealthMonitoringService));
			InitializeQueueMonitoringService(e.Tenant);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, WorkerService>();
			services.AddSingleton<IHostedService, SubscriptionWorker>();
			services.AddSingleton<IHostedService, SubscriptionEventWorker>();
			services.AddSingleton<IHostedService, QueueWorkerService>();
		}

		private void InitializeQueueMonitoringService(ITenant tenant)
		{
			var service = new QueueMonitoringService();

			Func<MiddlewareHealthMonitoringConfiguration> getConfig = () => Shell.GetConfiguration<IClientSys>().HealthMonitoredMiddleware.FirstOrDefault(e => string.Compare(e.Type, "System/QueueMonitoring") == 0);

			var timeout = 10;

			service.OnTimeout += (sender, snapshot) =>
			{
				try
				{
					var history = (sender as IQueueMonitoringService).GetHistory();

					var lastFewEntries = history.TakeLast(timeout);

					if (!lastFewEntries.Any())
						return;

					if (getConfig() is not MiddlewareHealthMonitoringConfiguration config)
						return;

					var enqueued = lastFewEntries.Select(e => e.Enqueued);

					var enqueuedStuck = enqueued.Sum() == 0;

					var processed = lastFewEntries.Select(e => e.Processed);

					var processedStuck = processed.Sum() == 0;

					var healthMonitoringService = Tenant.GetService<IHealthMonitoringService>();
					var queueWorkerService = QueueWorkerService.ServiceInstance;

					/*
					 * If nothing is moving and the available slots are full, send warning.
					 */
					if (enqueuedStuck && processedStuck && (queueWorkerService?.Dispatchers.Select(e => e.Available).DefaultIfEmpty(int.MinValue).Sum() ?? int.MinValue) == 0)
						healthMonitoringService?.TryLog($"No elements have been enqueued or processed for the past {timeout} seconds.", TraceLevel.Error, config);
				}
				catch { }
			};


			tenant.RegisterService(typeof(IQueueMonitoringService), service);
		}
	}
}
