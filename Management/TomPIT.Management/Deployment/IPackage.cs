using System.Collections.Generic;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.Deployment
{
	public interface IPackage
	{
		IPackageMetaData MetaData { get; }
		IPackageMicroService MicroService { get; }
		List<IMicroServiceString> Strings { get; }
		List<IPackageFeature> Features { get; }
		List<IPackageComponent> Components { get; }
		List<IPackageBlob> Blobs { get; }
		List<IPackageDependency> Dependencies { get; }
		List<IDatabase> Databases { get; }
	}
}
