using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageConfiguration : IPackageConfiguration
	{
		private List<IPackageConfigurationDatabase> _databases = null;
		private List<IPackageConfigurationDependency> _dependencies = null;

		public Guid ResourceGroup { get; set; }
		public bool RuntimeConfigurationSupported { get; set; }
		public bool RuntimeConfiguration { get; set; }
		public bool AutoVersioning { get; set; } = true;

		public List<IPackageConfigurationDatabase> Databases
		{
			get
			{
				if (_databases == null)
					_databases = new List<IPackageConfigurationDatabase>();

				return _databases;
			}
		}

		public List<IPackageConfigurationDependency> Dependencies
		{
			get
			{
				if (_dependencies == null)
					_dependencies = new List<IPackageConfigurationDependency>();

				return _dependencies;
			}
		}
	}
}
