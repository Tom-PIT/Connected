using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TomPIT.Runtime
{
	public interface IInstanceStartup
	{
		void ConfigureServices(IServiceCollection services);
		void Initialize();
		void Configure(IApplicationBuilder app, IWebHostEnvironment env);
		void ConfigureRouting(ConfigureRoutingArgs e);
		void ConfigureMiddleware(ConfigureMiddlewareArgs e);
	}
}
