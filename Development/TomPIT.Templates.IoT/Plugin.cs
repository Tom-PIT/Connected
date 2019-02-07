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
				"TomPIT.IoT.Views"
			};
		}

		public void Initialize()
		{

		}
	}
}
