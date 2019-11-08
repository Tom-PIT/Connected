using System;
using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PackageFolder : IPackageFolder
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
		[JsonProperty(PropertyName = "parent")]
		public Guid Parent { get; set; }
	}
}
