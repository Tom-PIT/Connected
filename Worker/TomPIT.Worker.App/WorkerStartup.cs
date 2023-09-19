using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TomPIT.Connectivity;
using TomPIT.Startup;
using TomPIT.Worker.HostedServices;
using TomPIT.Worker.Services;
using TomPIT.Worker.Subscriptions;

namespace TomPIT.Worker
{
    public class WorkerStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.Booting += OnBooting;
            host.ConfiguringServices += OnConfiguringServices;
            host.ConfiguringRouting += OnConfiguringRouting;
            host.Configuring += OnConfiguring;
        }

        private void OnConfiguring(object sender, System.Tuple<IApplicationBuilder, IWebHostEnvironment> e)
        {
            Tenant.GetService<IHostedServices>().Initialize();

            e.Item1.ApplicationServices.GetService<IQueueMonitoringService>();
        }

        private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
        {
            Configuration.Routing.Register(e);
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
            e.AddSingleton<IHostedService, WorkerService>();
            e.AddSingleton<IHostedService, SubscriptionWorker>();
            e.AddSingleton<IHostedService, SubscriptionEventWorker>();
            e.AddSingleton<IHostedService, QueueWorkerService>();
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(ISubscriptionWorkerService), typeof(SubscriptionWorkerService));
            e.Tenant.RegisterService(typeof(IWorkerProxyService), typeof(WorkerProxyService));
            e.Tenant.RegisterService(typeof(IHostedServices), typeof(HostedServicesContainer));
        }
    }
}
