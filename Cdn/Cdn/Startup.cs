using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Cdn.Mail;
using TomPIT.Cdn.Routing;
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
				Authentication = AuthenticationType.SingleTenant
			};

			Instance.Initialize(services, e);
			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(InstanceType.Cdn, app, env, (f) =>
			{
				CdnRouting.Register(f.Builder);
			});
			Instance.Run(app);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, MailService>();
			services.AddSingleton<IHostedService, SmtpConnectionCleanupService>();
		}
	}
}
