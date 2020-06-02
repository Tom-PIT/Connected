using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Cdn.Data;
using TomPIT.Cdn.Mail;
using TomPIT.Cdn.Printing;
using TomPIT.Cdn.Routing;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Middleware;
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
				Authentication = AuthenticationType.SingleTenant
			};

			Instance.Initialize(services, e);

			services.AddCors(options => options.AddPolicy("TomPITPolicy",
				builder =>
				{
					var setting = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().Select(Guid.Empty, "Cors Origins");
					var origin = new string[] { "http://localhost" };

					if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
						origin = setting.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

					builder.AllowAnyMethod()
					.AllowAnyHeader()
					.WithOrigins(origin)
					.AllowCredentials();
				}));

			services.AddSignalR(o =>
			{
				o.EnableDetailedErrors = true;
			}).AddNewtonsoftJsonProtocol();

			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(InstanceType.Cdn, app, env, (f) =>
			{
				CdnRouting.Register(f.Builder);
			});

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Instance.Run(app);

			app.UseCors("TomPITPolicy");
			app.UseRouting();
			app.UseEndpoints(routes =>
			{
				routes.MapHub<DataHub>("/dataHub");
			});
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IDataHubService), typeof(DataHubService));
			e.Tenant.RegisterService(typeof(IPrintingManagementService), typeof(PrintingManagementService));
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, MailService>();
			services.AddSingleton<IHostedService, SmtpConnectionCleanupService>();
			services.AddSingleton<IHostedService, PrintingService>();
			services.AddSingleton<IHostedService, SmtpService>();
		}
	}
}
