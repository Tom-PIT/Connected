using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace TomPIT.Compilation
{
	internal class LocalRepository : SourceRepository
	{
		public LocalRepository(Guid blob) : base(new LocalPackageSource(), new List<INuGetResourceProvider> { new LocalResourceProvider(blob) })
		{
		}
	}
}
