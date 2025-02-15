﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Cdn.Documents;
using TomPIT.Runtime.Configuration;

namespace TomPIT.MicroServices.BigData.Runtime.Configuration
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

		public List<IDocumentProvider> GetDocumentProviders()
		{
			return null;
		}

		public void Initialize(IApplicationBuilder app, IWebHostEnvironment env)
		{

		}

		public void RegisterRoutes(IEndpointRouteBuilder builder)
		{
		}
	}
}
