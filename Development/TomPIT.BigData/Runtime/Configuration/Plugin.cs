using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Cdn;
using TomPIT.Runtime.Configuration;

namespace TomPIT.MicroServices.BigData
{
	internal class Plugin : IPlugin
	{
		public void ConfigureServices(IServiceCollection services)
		{
		}

		public List<string> GetApplicationParts(ApplicationPartManager manager)
		{
			return new List<string>
			{
				"TomPIT.MicroServices.BigData"
			};
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>
			{
				"TomPIT.MicroServices.BigData"
			};
		}

		public List<IPrintingProvider> GetPrintingProviders()
		{
			return null;
		}

		public void Initialize(IApplicationBuilder app, IWebHostEnvironment env)
		{

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
		}
	}
}
