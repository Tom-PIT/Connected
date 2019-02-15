using Newtonsoft.Json;
using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageMicroService : IPackageMicroService
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
		[JsonProperty(PropertyName = "template")]
		public Guid Template { get; set; }
		[JsonProperty(PropertyName = "metaData")]
		public string MetaData { get; set; }
	}
}
