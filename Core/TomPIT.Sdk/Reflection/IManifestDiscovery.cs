using System;
using System.Collections.Immutable;
using TomPIT.Reflection.Manifests;

namespace TomPIT.Reflection
{
	public interface IManifestDiscovery
	{
		IComponentManifest Select(Guid component);
		IComponentManifest Select(string microService, string category, string componentName);
		ImmutableList<IComponentManifest> Query(Guid microService);
	}
}
