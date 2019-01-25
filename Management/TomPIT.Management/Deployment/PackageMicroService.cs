using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
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
