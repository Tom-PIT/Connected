using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public interface IPackage
	{
		IPackageMetaData MetaData { get; }
		IPackageMicroService MicroService { get; }
		List<IPackageBlob> Blobs { get; }
		List<IPackageComponent> Components { get; }
		List<IPackageFolder> Folders { get; }
		List<IPackageString> Strings { get; }
		List<IPackageDependency> Dependencies { get; }
		IPackageConfiguration Configuration { get; }
		List<IPackageDatabase> Databases { get; }
	}
}
