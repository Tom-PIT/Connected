using System;
using TomPIT.Deployment;
using TomPIT.Environment;
using TomPIT.Management.Deployment;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Models
{
	public class DeploymentConfigurationModel
	{
		private IPackageConfiguration _configuration = null;
		private IResourceGroup _resourceGroup = null;

		public DeploymentConfigurationModel(Guid package, DeploymentDesigner designer)
		{
			Package = package;
			Designer = designer;
		}

		public Guid Package { get; }
		public DeploymentDesigner Designer { get; }
		public IPackageConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
				{
					if (Designer.Configurations.ContainsKey(Package))
						_configuration = Designer.Configurations[Package];
					else
					{
						_configuration = Designer.Environment.Context.Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(Package);
						Designer.Configurations[Package] = _configuration;
					}
				}

				return _configuration;
			}
		}

		public IResourceGroup ResourceGroup
		{
			get
			{
				if (_resourceGroup == null)
					_resourceGroup = Designer.Environment.Context.Tenant.GetService<IResourceGroupService>().Select(Configuration.ResourceGroup);

				return _resourceGroup;
			}
		}
	}
}
