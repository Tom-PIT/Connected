using System.Collections.Generic;
using TomPIT.Configuration;

namespace TomPIT.Application
{
	public class Plugin : IPlugin
	{
		public List<string> GetApplicationParts()
		{
			return new List<string>();
		}

		public void Initialize()
		{

		}
	}
}
