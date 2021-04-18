using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Cdn.Documents;
using TomPIT.Runtime.Configuration;

namespace TomPIT.MicroServices.IoT.Runtime.Configuration
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
				"TomPIT.MicroServices.IoT",
				"TomPIT.MicroServices.IoT.Views"
			};
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>
			{
				"TomPIT.MicroServices.IoT"
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
			//builder.MapRoute("sys/plugins/iot/partial/{id}", (t) =>
			//{
			//	return Task.CompletedTask;
			//});

			builder.MapControllerRoute("sys.plugins.iot.partial", "sys/plugins/iot/partial/{microService}/{view}", new { controller = "IoT", action = "Partial" }, null, new { Namespace = "TomPIT.MicroServices.IoT.Controllers" });
		}
	}
}
