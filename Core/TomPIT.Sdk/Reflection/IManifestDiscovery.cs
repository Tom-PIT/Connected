using System;
using System.Collections.Immutable;

namespace TomPIT.Reflection
{
	public interface IManifestDiscovery
	{
		IScriptManifest SelectScript(Guid microService, Guid component, Guid id);
		IComponentManifest Select(Guid component);
		IComponentManifest Select(string microService, string category, string componentName);
		ImmutableList<IComponentManifest> Query(Guid microService);

		IManifestTypeResolver SelectTypeResolver(IManifestMiddleware manifest);
		IManifestTypeResolver SelectTypeResolver(Guid microService, Guid component, Guid script);
	}
}
