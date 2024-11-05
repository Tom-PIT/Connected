using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy
{
	public interface IMicroServiceController
	{
		IMicroService SelectByUrl(string url);
		IMicroService Select(Guid microService);
		IMicroService Select(string name);

		ImmutableList<IMicroService> Query();
	}
}
