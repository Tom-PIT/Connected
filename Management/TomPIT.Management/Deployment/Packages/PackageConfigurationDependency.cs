using System;
using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PackageConfigurationDependency : IPackageConfigurationDependency
	{
		[JsonProperty("dependency")]
		public Guid Dependency { get; set; }
		[JsonProperty("enabled")]
		public bool Enabled { get; set; }
	}
}
