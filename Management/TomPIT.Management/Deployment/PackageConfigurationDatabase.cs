using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageConfigurationDatabase : IPackageConfigurationDatabase
	{
		public string ConnectionString { get; set; }
		public Guid Connection { get; set; }
	}
}
