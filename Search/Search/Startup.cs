using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.Search.Routing;
using TomPIT.Search.Services;

namespace TomPIT.Search
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

			Instance.Initialize(InstanceType.Search, services, e);
			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(app, env, (f) =>
			{
				SearchRouting.Register(f.Builder);
			});

			InitializeConfiguration();
			Instance.Run(app, env);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, Services.IndexingService>();
			services.AddSingleton<IHostedService, ScaveningService>();
			services.AddSingleton<IHostedService, FlushingService>();
		}

		private void InitializeConfiguration()
		{
			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(IIndexingService), typeof(IndexingService));
		}
	}
}
