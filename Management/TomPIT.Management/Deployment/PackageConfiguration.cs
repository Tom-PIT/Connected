using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageConfiguration : IPackageConfiguration
	{
		private List<IPackageConfigurationDatabase> _databases = null;

		public Guid ResourceGroup { get; set; }
		public bool RuntimeConfigurationSupported { get; set; }
		public bool RuntimeConfiguration { get; set; }

		public List<IPackageConfigurationDatabase> Databases
		{
			get
			{
				if (_databases == null)
					_databases = new List<IPackageConfigurationDatabase>();

				return _databases;
			}
		}
	}
}
