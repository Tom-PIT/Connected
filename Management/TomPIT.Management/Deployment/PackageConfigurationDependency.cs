using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageConfigurationDependency : IPackageConfigurationDependency
	{
		[JsonProperty("dependency")]
		public Guid Dependency { get; set; }
		[JsonProperty("enabled")]
		public bool Enabled { get; set; }
	}
}
