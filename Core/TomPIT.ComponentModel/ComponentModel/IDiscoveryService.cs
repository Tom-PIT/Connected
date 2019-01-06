using System;

namespace TomPIT.ComponentModel
{
	public interface IDiscoveryService
	{
		IServiceReferences References(string microService);
		IServiceReferences References(Guid microService);
		IElement Find(Guid component, Guid id);
	}
}
