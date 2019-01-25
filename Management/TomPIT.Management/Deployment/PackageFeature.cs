using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
{
	internal class PackageFeature : IPackageFeature
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
	}
}
