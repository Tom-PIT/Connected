using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection
{
	public interface IMicroServiceReferencesDiscovery
	{
		IServiceReferencesConfiguration Select(string microService);
		IServiceReferencesConfiguration Select(Guid microService);
		List<IMicroService> Flatten(Guid microService);
	}
}
