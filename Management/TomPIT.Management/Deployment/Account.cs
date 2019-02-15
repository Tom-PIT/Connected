using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class Account : IAccount
	{
		[JsonProperty(PropertyName = "company")]
		public string Company { get; set; }
		[JsonProperty(PropertyName = "country")]
		public int Country { get; set; }
		[JsonProperty(PropertyName = "website")]
		public string Website { get; set; }
		[JsonProperty(PropertyName = "publisherKey")]
		public string Key { get; set; }
		[JsonProperty(PropertyName = "status")]
		public AccountStatus Status { get; set; }
	}
}
