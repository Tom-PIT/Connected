using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.BigData.Connectivity;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.BigData.Providers.Sql;
using TomPIT.BigData.Transactions;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagnostics.Tracing;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.BigData
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

			Instance.Initialize(InstanceType.BigData, services, e);
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Instance.InitializeShellServices();

			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(app, env, (f) =>
			{
				f.Builder.MapHub<TraceHub>("hubs/tracing");
				BigData.Configuration.Routing.Register(f.Builder);				
			}, 
			(f)=>
			{
				var traceHubContext = f.Builder.ApplicationServices.GetRequiredService<IHubContext<TraceHub>>();

				var traceService = MiddlewareDescriptor.Current.Tenant.GetService<ITraceService>();

				traceService.TraceReceived += async (s, e) => await TraceHub.Trace(traceHubContext, e);
				traceService.TraceReceived += async (s, e) => Tenant.GetService<ILoggingService>().Dump($"{e.Endpoint.Identifier} {e.Content}");
			});

			Instance.Run(app, env);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, StorageService>();
			services.AddSingleton<IHostedService, MaintenanceService>();
			services.AddSingleton<IHostedService, BufferingWorker>();
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(ITraceService), typeof(TraceService));
			e.Tenant.RegisterService(typeof(INodeService), typeof(NodeService));
			e.Tenant.RegisterService(typeof(ITransactionService), typeof(TransactionService));
			e.Tenant.RegisterService(typeof(IPartitionService), typeof(PartitionService));
			e.Tenant.RegisterService(typeof(IPersistenceService), typeof(SqlPersistenceService));
			e.Tenant.RegisterService(typeof(IPartitionMaintenanceService), typeof(PartitionMaintenanceService));
			e.Tenant.RegisterService(typeof(IBufferingService), typeof(BufferingService));
			e.Tenant.RegisterService(typeof(ITimeZoneService), typeof(TimeZoneService));

			e.Tenant.Items.TryAdd("bigdataClient", new BigDataClient(e.Tenant, e.Tenant.AuthenticationToken));
		}
	}
}
