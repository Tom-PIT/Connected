using System.Collections.Generic;

namespace TomPIT.Configuration
{
	public interface IPlugin
	{
		void Initialize();

		List<string> GetApplicationParts();
	}
}
