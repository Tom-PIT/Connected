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

			Instance.Initialize(services, e);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Instance.Configure(InstanceType.Rest, app, env, (f) =>
		  {
			  RestRouting.Register(f.Builder);
		  });

			Instance.Run(app, env);
		}
	}
}
