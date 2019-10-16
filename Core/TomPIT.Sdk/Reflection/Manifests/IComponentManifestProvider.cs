using System;
using TomPIT.Connectivity;

namespace TomPIT.Reflection.Manifests
{
	public interface IComponentManifestProvider
	{
		IComponentManifest CreateManifest(ITenant tenant, Guid component);
		IComponentManifest CreateManifest(ITenant tenant, Guid component, Guid element);
	}
}
