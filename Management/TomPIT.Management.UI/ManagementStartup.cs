using Microsoft.Extensions.DependencyInjection;
using TomPIT.Connectivity;
using TomPIT.Ide;
using TomPIT.Management.Deployment;
using TomPIT.Startup;

namespace TomPIT.Management
{
    public class ManagementStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.ConfiguringServices += OnConfiguringServices;
            host.Booting += OnBooting;
            host.ConfiguringRouting += OnConfiguringRouting;
            host.ConfigureEmbeddedStaticResources += OnConfigureEmbeddedStaticResources;
        }

        private void OnConfigureEmbeddedStaticResources(object sender, System.Collections.Generic.List<System.Reflection.Assembly> e)
        {
            e.Add(typeof(ManagementStartup).Assembly);
        }

        private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
        {
            IdeRouting.Register(e, "Management", "mng");
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            IdeBootstrapper.Initialize(sender, e);
            ManagementBootstrapper.Initialize(e.Tenant);
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
            e.AddHostedService<InstallerService>();
            e.AddHostedService<UpdateService>();
            e.AddHostedService<PublishService>();
        }
    }
}
