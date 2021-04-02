using System;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	public interface IComponentManifestProvider
	{
		IComponentManifest CreateManifest(ITenant tenant, Guid component);
		IComponentManifest CreateManifest(ITenant tenant, Guid component, Guid element);

		void LoadMetaData(IComponentManifest manifest);
	}
}
