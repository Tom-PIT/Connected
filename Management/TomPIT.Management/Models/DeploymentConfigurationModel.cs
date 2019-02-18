using System;
using System.Collections.Generic;
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
					_configuration = Designer.Environment.Context.Connection().GetService<IDeploymentService>().SelectInstallerConfiguration(Package);

				return _configuration;
			}
		}

		public IResourceGroup ResourceGroup
		{
			get
			{
				if (_resourceGroup == null)
					_resourceGroup = Designer.Environment.Context.Connection().GetService<IResourceGroupService>().Select(Configuration.ResourceGroup);

				return _resourceGroup;
			}
		}
	}
}
