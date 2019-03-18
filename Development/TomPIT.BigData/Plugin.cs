using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Configuration;

namespace TomPIT.BigData
{
	internal class Plugin : IPlugin
	{
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

		public void Initialize()
		{

		}

		public void RegisterRoutes(IRouteBuilder builder)
		{
		}
	}
}
