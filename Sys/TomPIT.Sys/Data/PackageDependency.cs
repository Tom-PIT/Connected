using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data
{
	internal class PackageDependency : IPackageDependency
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("token")]
		public Guid Token { get; set; }
	}
}
