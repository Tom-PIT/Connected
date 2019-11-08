using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TomPIT.Runtime.Configuration
{
	public interface IPlugin
	{
		void Initialize(IApplicationBuilder app, IWebHostEnvironment env);
		void ConfigureServices(IServiceCollection services);

		List<string> GetApplicationParts(ApplicationPartManager manager);
		List<string> GetEmbeddedResources();
		void RegisterRoutes(IRouteBuilder builder);
	}
}
