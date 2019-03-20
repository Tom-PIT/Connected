using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.BigData.Connectivity;
using TomPIT.BigData.Services;
using TomPIT.Connectivity;
using TomPIT.Environment;

namespace TomPIT.BigData
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
			Instance.Configure(InstanceType.BigData, app, env, (f) =>
			{
				BigData.Configuration.Routing.Register(f.Builder);
			});
			InitializeConfiguration();
			Instance.Run(app);
		}

		private void RegisterTasks(IServiceCollection services)
		{
			//services.AddSingleton<IHostedService, MailService>();
			//services.AddSingleton<IHostedService, ConnectionCleanupService>();
		}

		private void InitializeConfiguration()
		{
			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
		}

		private void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(INodeService), typeof(NodeService));
			e.Connection.RegisterService(typeof(ITransactionService), typeof(TransactionService));

			e.Connection.Items.TryAdd("bigdataClient", new BigDataClient(e.Connection, e.Connection.AuthenticationToken));
		}
	}
}
