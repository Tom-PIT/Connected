using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Analysis.Manifest
{
	public interface IComponentManifestProvider
	{
		IComponentManifest CreateManifest(ISysConnection connection, Guid component);
	}
}
