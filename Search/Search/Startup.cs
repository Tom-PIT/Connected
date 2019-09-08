using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT;
using TomPIT.Connectivity;
using TomPIT.Environment;
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

			Instance.Initialize(services, e);
			RegisterTasks(services);
		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Search, app, env, (f) =>
			{
				Search.Configuration.Routing.Register(f.Builder);
			});

			InitializeConfiguration();
			Instance.Run(app);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			services.AddSingleton<IHostedService, Services.IndexingService>();
			services.AddSingleton<IHostedService, ScaveningService>();
			services.AddSingleton<IHostedService, FlushingService>();
		}

		private void InitializeConfiguration()
		{
			Shell.GetService<IConnectivityService>().ConnectionInitialize += OnConnectionInitialize;
		}

		private void OnConnectionInitialize(object sender, SysConnectionArgs e)
		{
			e.Connection.RegisterService(typeof(IIndexingService), typeof(IndexingService));
		}
	}
}
