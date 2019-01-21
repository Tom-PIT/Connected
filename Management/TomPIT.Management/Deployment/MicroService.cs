using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
{
	public class MicroService
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }
		[JsonProperty(PropertyName = "template")]
		public Guid Template { get; set; }
		[JsonProperty(PropertyName = "meta")]
		public string Meta { get; set; }
	}
}
