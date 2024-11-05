using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace TomPIT.Compilation
{
	internal class LocalPackageIdentity : SourcePackageDependencyInfo
	{
		public LocalPackageIdentity(PackageArchiveReader reader, Guid blob, List<PackageDependency> dependencies) :
			base(reader.NuspecReader.GetId(), reader.NuspecReader.GetVersion(), dependencies, true, new LocalRepository(blob), new Uri(ResolveDownloadUrl(reader)), null)
		{

			Blob = blob;
		}
		public Guid Blob { get; }

		private static string ResolveDownloadUrl(PackageArchiveReader reader)
		{
			var id = reader.NuspecReader.GetId();
			var version = reader.NuspecReader.GetVersion().ToString();

			return $"https://localhost/tompit/{id}/{version}/{id}.{version}.nupkg";
		}
	}
}
