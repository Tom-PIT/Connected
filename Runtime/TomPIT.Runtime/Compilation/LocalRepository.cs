using System;
using System.Collections.Generic;
using NuGet.Protocol.Core.Types;
using TomPIT.Connectivity;

namespace TomPIT.Compilation
{
	internal class LocalRepository : SourceRepository
	{
		public LocalRepository(ITenant tenant, Guid blob) : base(new LocalPackageSource(), new List<INuGetResourceProvider> { new LocalResourceProvider(tenant, blob) })
		{
		}
	}
}
