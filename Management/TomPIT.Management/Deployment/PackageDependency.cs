using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageDependency : IPackageDependency
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
	}
}
