using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TomPIT.Environment;

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

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			Instance.Configure(InstanceType.Rest, app, env, (f) =>
		  {
			  f.Builder.MapRoute("{microservice}/{api}/{operation}", (t) =>
			  {
				  new ApiHandler(t, Instance.Connection.Url).Invoke();

				  return Task.CompletedTask;
			  });
		  });

			Instance.Run(app);
		}
	}
}
