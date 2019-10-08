using System;
using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PackageComponent : IPackageComponent
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		[JsonProperty(PropertyName = "folder")]
		public Guid Folder { get; set; }
		[JsonProperty(PropertyName = "category")]
		public string Category { get; set; }
		[JsonProperty(PropertyName = "runtimeConfiguration")]
		public Guid RuntimeConfiguration { get; set; }
	}
}
