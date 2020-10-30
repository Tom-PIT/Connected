using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Cdn;

namespace TomPIT.Runtime.Configuration
{
	public interface IPlugin
	{
		void Initialize(IApplicationBuilder app, IWebHostEnvironment env);
		void ConfigureServices(IServiceCollection services);

		List<string> GetApplicationParts(ApplicationPartManager manager);
		List<string> GetEmbeddedResources();
		List<IPrintingProvider> GetPrintingProviders();
		void RegisterRoutes(IEndpointRouteBuilder builder);
	}
}
