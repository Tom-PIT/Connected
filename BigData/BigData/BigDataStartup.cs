using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
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
using TomPIT.Middleware;
using TomPIT.Startup;

namespace TomPIT.BigData
{
    public class BigDataStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.Booting += OnBooting;
            host.ConfiguringServices += OnConfiguringServices;
            host.ConfiguringRouting += OnConfiguringRouting;
            host.Configuring += OnConfiguring;
        }

        private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
        {
            e.MapHub<TraceHub>("hubs/tracing");

            Configuration.Routing.Register(e);
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
         e.AddSingleton<IHostedService, StorageService>();
         e.AddSingleton<IHostedService, MaintenanceService>();
         e.AddSingleton<IHostedService, BufferingWorker>();
         //e.AddSingleton<ITraceService, TraceService>();
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnConfiguring(object sender, System.Tuple<IApplicationBuilder, IWebHostEnvironment> e)
        {
            //var traceService = e.Item1.ApplicationServices.GetService<ITraceService>();

            //MiddlewareDescriptor.Current.Tenant.RegisterService(typeof(ITraceService), traceService);
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
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
