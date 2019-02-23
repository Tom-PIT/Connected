using TomPIT.Deployment;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Models
{
	public class DeploymentPackageCardModel
	{
		public DeploymentPackageCardModel(DeploymentDesigner designer, IPublishedPackage package)
		{
			Designer = designer;
			Package = package;
		}
		public IPublishedPackage Package { get; }
		public DeploymentDesigner Designer { get; }
	}
}
