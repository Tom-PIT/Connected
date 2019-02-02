using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
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
