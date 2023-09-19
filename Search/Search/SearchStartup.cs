using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Connectivity;
using TomPIT.Search.Routing;
using TomPIT.Search.Services;
using TomPIT.Startup;

namespace TomPIT.Search
{
    public class SearchStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.ConfiguringServices += OnConfiguringServices;
            host.Booting += OnBooting;
            host.ConfiguringRouting += OnConfiguringRouting;
        }

        private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
        {
            SearchRouting.Register(e);
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
            e.AddSingleton<IHostedService, Services.IndexingService>();
            e.AddSingleton<IHostedService, ScaveningService>();
            e.AddSingleton<IHostedService, FlushingService>();
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(IIndexingService), typeof(IndexingService));
        }
    }
}
