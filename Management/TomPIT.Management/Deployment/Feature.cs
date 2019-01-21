using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
{
	public class Feature
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "token")]
		public Guid Token { get; set; }
	}
}
