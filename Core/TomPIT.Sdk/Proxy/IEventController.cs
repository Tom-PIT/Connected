using System;

namespace TomPIT.Proxy
{
	public interface IEventController
	{
		Guid Trigger(Guid microService, string name, string callback, string arguments);
	}
}
