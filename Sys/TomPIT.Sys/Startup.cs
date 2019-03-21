using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Sys.Configuration;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.Services;
using TomPIT.Sys.Workers;

namespace TomPIT.Sys
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
			Shell.RegisterConfigurationType(typeof(ServerSys));

			services.AddMvc();

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = "TomPIT";
				options.DefaultChallengeScheme = "TomPIT";
				options.DefaultScheme = "TomPIT";
			}).AddTomPITAuthentication("TomPIT", "Tom PIT", o =>
			{

			});

			services.AddSignalR(o =>
			{
				o.EnableDetailedErrors = true;
			});

			RegisterTasks(services);

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseTomPITExceptionMiddleware();
			}
			else
			{
				app.UseTomPITExceptionMiddleware();
				//app.UseExceptionHandler();
			}

			app.UseSignalR(routes =>
			{
				routes.MapHub<CacheHub>("/caching");
				routes.MapHub<IoTHub>("/iot");
				routes.MapHub<BigDataHub>("/bigdata");
			});

			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller}/{action}");
			});

			app.UseAuthentication();

			ServerConfiguration.Initialize();
			Shell.Configure(app);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, MessageDispatcher>();
			services.AddSingleton<IHostedService, MessageDisposer>();
			services.AddSingleton<IHostedService, Scheduler>();
			services.AddSingleton<IHostedService, Preloader>();
			//services.AddSingleton<IHostedService, QueueService>();
		}
	}
}
