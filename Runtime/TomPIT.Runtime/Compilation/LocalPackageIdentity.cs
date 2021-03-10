using System;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using TomPIT.Connectivity;

namespace TomPIT.Compilation
{
	internal class LocalPackageIdentity : PackageIdentity
	{
		public LocalPackageIdentity(string id, NuGetVersion version, Guid blob, ITenant tenant) : base(id, version)
		{
			Blob = blob;
			Tenant = tenant;
		}
		public Guid Blob { get; }
		public ITenant Tenant { get; }
	}
}
