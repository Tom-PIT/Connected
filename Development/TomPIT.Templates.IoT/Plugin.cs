using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Configuration;

namespace TomPIT.IoT
{
	internal class Plugin : IPlugin
	{
		public List<string> GetApplicationParts()
		{
			return new List<string>
			{
				"TomPIT.IoT.Views"
			};
		}

		public void Initialize()
		{

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
			builder.MapRoute("sys/plugins/iot/partial/{id}", (t) =>
			{
				//new GlobalizationHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}
