using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Cdn.Clients;
using TomPIT.Cdn.Events;
using TomPIT.Cdn.Mail;
using TomPIT.Cdn.Printing;
using TomPIT.Cdn.Routing;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Runtime;

namespace TomPIT.Cdn
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

			Instance.Initialize(InstanceType.Cdn, services, e);
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Instance.InitializeShellServices();

			services.AddSignalR(o =>
			{
				o.EnableDetailedErrors = true;
			}).AddNewtonsoftJsonProtocol();

			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(app, env, (f) =>
			{
				CdnRouting.Register(f.Builder);

				f.Builder.MapHub<EventHub>("/events");
				f.Builder.MapHub<PrintingHub>("/printing");
				f.Builder.MapHub<ClientHub>("/clients");
			});

			Instance.Run(app, env);
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IEventHubService), typeof(EventHubService));
			e.Tenant.RegisterService(typeof(IPrintingManagementService), typeof(PrintingManagementService));
			e.Tenant.RegisterService(typeof(IPrintingSpoolerManagementService), typeof(PrintingSpoolerManagementService));
			e.Tenant.RegisterService(typeof(IInboxService), typeof(InboxService));
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, MailService>();
			services.AddSingleton<IHostedService, SmtpConnectionCleanupService>();
			services.AddSingleton<IHostedService, PrintingService>();
			services.AddSingleton<IHostedService, SmtpService>();
			services.AddSingleton<IHostedService, EventService>();
			services.AddSingleton<IHostedService, EventReliableService>();
			services.AddSingleton<IHostedService, PrintingSpoolerService>();
		}
	}
}
