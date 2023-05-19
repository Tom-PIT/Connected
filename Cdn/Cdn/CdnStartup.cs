using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Cdn.Clients;
using TomPIT.Cdn.Events;
using TomPIT.Cdn.Mail;
using TomPIT.Cdn.Printing;
using TomPIT.Cdn.Routing;
using TomPIT.Connectivity;
using TomPIT.Startup;

namespace TomPIT.Cdn
{
    public class CdnStartup : IStartupClient
    {
        public void Initialize(IStartupHost host)
        {
            host.Booting += OnBooting;
            host.ConfiguringServices += OnConfiguringServices;
            host.ConfiguringRouting += OnConfiguringRouting;
        }

        private void OnConfiguringRouting(object sender, Microsoft.AspNetCore.Routing.IEndpointRouteBuilder e)
        {
            CdnRouting.Register(e);

            e.MapHub<EventHub>("/events");
            e.MapHub<PrintingHub>("/printing");
            e.MapHub<ClientHub>("/clients");
        }

        private void OnConfiguringServices(object sender, IServiceCollection e)
        {
            e.AddSingleton<IHostedService, MailService>();
            e.AddSingleton<IHostedService, SmtpConnectionCleanupService>();
            e.AddSingleton<IHostedService, PrintingService>();
            e.AddSingleton<IHostedService, SmtpService>();
            e.AddSingleton<IHostedService, EventService>();
            e.AddSingleton<IHostedService, EventReliableService>();
            e.AddSingleton<IHostedService, EventCleanupService>();
            e.AddSingleton<IHostedService, PrintingSpoolerService>();
        }

        private void OnBooting(object sender, System.EventArgs e)
        {
            Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
        }

        private void OnTenantInitialize(object sender, TenantArgs e)
        {
            e.Tenant.RegisterService(typeof(IEventHubService), typeof(EventHubService));
            e.Tenant.RegisterService(typeof(IPrintingManagementService), typeof(PrintingManagementService));
            e.Tenant.RegisterService(typeof(IPrintingSpoolerManagementService), typeof(PrintingSpoolerManagementService));
            e.Tenant.RegisterService(typeof(IInboxService), typeof(InboxService));
        }
    }
}
