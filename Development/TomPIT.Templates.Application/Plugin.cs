using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TomPIT.Configuration;

namespace TomPIT.Application
{
	public class Plugin : IPlugin
	{
		public void ConfigureServices(IServiceCollection services)
		{
		}

		public List<string> GetApplicationParts(ApplicationPartManager manager)
		{
			return new List<string>();
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>();
		}

		public void Initialize(IApplicationBuilder app, IHostingEnvironment env)
		{

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
		}
	}
}
