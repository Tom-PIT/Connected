using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace TomPIT.Configuration
{
	public interface IPlugin
	{
		void Initialize();

		List<string> GetApplicationParts();
		List<string> GetEmbeddedResources();
		void RegisterRoutes(IRouteBuilder builder);
	}
}
