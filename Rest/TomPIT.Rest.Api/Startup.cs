using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Environment;
using TomPIT.Rest.Routing;
using TomPIT.Runtime;

namespace TomPIT.Rest
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			var e = new ServicesConfigurationArgs
			{
				Authentication = AuthenticationType.SingleTenant
			};

			Instance.Initialize(InstanceFeatures.Rest, services, e);
			Instance.InitializeShellServices();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(app, env, (f) =>
				{
					RestRouting.Register(f.Builder);
				}
			);

			Instance.Run(app, env);
		}
	}
}
