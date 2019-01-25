using Newtonsoft.Json;

namespace TomPIT.Marketplace
{
	internal class Publisher : IPublisher
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
		public PublisherStatus Status { get; set; }
	}
}
