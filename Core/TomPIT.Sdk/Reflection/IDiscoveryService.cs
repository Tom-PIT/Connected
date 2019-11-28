using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Reflection.Manifests;

namespace TomPIT.Reflection
{
	public interface IDiscoveryService
	{
		IServiceReferencesConfiguration References(string microService);
		IServiceReferencesConfiguration References(Guid microService);
		IElement Find(Guid component, Guid id);
		IElement Find(IConfiguration configuration, Guid id);

		IComponentManifest Manifest(Guid component);
		IComponentManifest Manifest(string microService, string category, string componentName);
		List<IComponentManifest> Manifests(Guid microService);

		List<IMicroService> FlattenReferences(Guid microService);
	}
}
