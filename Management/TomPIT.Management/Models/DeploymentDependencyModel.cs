using System.Linq;
using TomPIT.Deployment;
using TomPIT.Management.Deployment;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Models
{
	public class DeploymentDependencyModel
	{
		private IPackageConfiguration _configuration = null;
		private IPublishedPackage _packageInfo = null;

		public DeploymentDependencyModel(DeploymentDesigner designer, IPackageDependency dependency)
		{
			Designer = designer;
			Dependency = dependency;
		}

		public DeploymentDesigner Designer { get; }
		public IPackageDependency Dependency { get; }

		public IPublishedPackage Package
		{
			get
			{
				if (_packageInfo == null)
					_packageInfo = Designer.DependencyPackages.FirstOrDefault(f=>f.Service == Dependency.MicroService);

				return _packageInfo;
			}
		}

		public IPackageConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Designer.Environment.Context.Connection().GetService<IDeploymentService>().SelectInstallerConfiguration(Designer.PackageInfo.Token);

				return _configuration;
			}
		}

		public bool IsDependencyEnabled
		{
			get
			{
				var d = Configuration.Dependencies.FirstOrDefault(f => f.Dependency == Package.Token);

				if (d != null)
					return d.Enabled;
				/*
				 * dependency is included by default
				 */
				return true;
			}
		}
	}
}
