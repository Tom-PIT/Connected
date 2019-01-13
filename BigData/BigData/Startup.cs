using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.BigData.Formatters;
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
				Authentication = AuthenticationType.Bearer,
				ConfigureMvc = (o) =>
				 {
					 o.OutputFormatters.Add(new ProtobufFormatter());
				 }
			};

			Instance.Initialize(services, e);
		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.BigData, app, env, null);
			Instance.Run(app);
		}
	}
}
