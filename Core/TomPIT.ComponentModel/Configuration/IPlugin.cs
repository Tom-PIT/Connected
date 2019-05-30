using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace TomPIT.Configuration
{
	public interface IPlugin
	{
		void Initialize(IApplicationBuilder app, IHostingEnvironment env);
		void ConfigureServices(IServiceCollection services);

		List<string> GetApplicationParts(ApplicationPartManager manager);
		List<string> GetEmbeddedResources();
		void RegisterRoutes(IRouteBuilder builder);
	}
}
