using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Configuration;

namespace TomPIT.BigData
{
	internal class Plugin : IPlugin
	{
		public void ConfigureServices(IServiceCollection services)
		{
		}

		public List<string> GetApplicationParts()
		{
			return new List<string>
			{
				"TomPIT.BigData"
			};
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>
			{
				"TomPIT.BigData"
			};
		}

		public void Initialize(IApplicationBuilder app, IHostingEnvironment env)
		{

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
		}
	}
}
