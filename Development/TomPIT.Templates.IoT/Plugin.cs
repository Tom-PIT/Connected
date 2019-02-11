using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using TomPIT.Configuration;

namespace TomPIT.IoT
{
	internal class Plugin : IPlugin
	{
		public List<string> GetApplicationParts()
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

		public void Initialize()
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
