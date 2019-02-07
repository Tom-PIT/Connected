using System;
using TomPIT.ComponentModel;

namespace TomPIT.Analysis
{
	public interface IDiscoveryService
	{
		IServiceReferences References(string microService);
		IServiceReferences References(Guid microService);
		IElement Find(Guid component, Guid id);
		IElement Find(IConfiguration configuration, Guid id);
	}
}
