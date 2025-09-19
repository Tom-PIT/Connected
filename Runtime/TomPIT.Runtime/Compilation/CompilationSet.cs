using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Compilation;
internal class CompilationSet : Queue<IMicroService>
{
	public CompilationSet()
	{
		Initialize();
	}

	private void Initialize()
	{
		var microServices = Tenant.GetService<IMicroServiceService>().Query();

		foreach (var microService in microServices)
			Initialize(microService);
	}

	private void Initialize(IMicroService microService)
	{
		var references = Tenant.GetService<IDiscoveryService>().MicroServices.References.References(microService.Token, false);

		foreach (var reference in references)
			Initialize(reference);

		if (!Contains(microService))
			Enqueue(microService);
	}
}
