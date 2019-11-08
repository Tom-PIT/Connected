using TomPIT.Deployment;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Deployment.Packages
{
	public interface IPackageStateDescriptor : IPublishedPackage
	{
		PackageState State { get; set; }
	}
}
