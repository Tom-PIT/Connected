using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TomPIT.Configuration;

namespace TomPIT.IoT
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
				"TomPIT.IoT",
				"TomPIT.IoT.Views"
			};
		}

		public List<string> GetEmbeddedResources()
		{
			return new List<string>
			{
				"TomPIT.IoT"
			};
		}

		public void Initialize(IApplicationBuilder app, IHostingEnvironment env)
		{

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
			//builder.MapRoute("sys/plugins/iot/partial/{id}", (t) =>
			//{
			//	return Task.CompletedTask;
			//});

			builder.MapRoute("sys.plugins.iot.partial", "sys/plugins/iot/partial/{microService}/{view}", new { controller = "IoT", action = "Partial" }, null, new { Namespace = "TomPIT.IoT.Controllers" });
		}
	}
}
