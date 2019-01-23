using Newtonsoft.Json;

namespace TomPIT.Marketplace
{
	internal class Publisher : IPublisher
	{
		[JsonProperty(PropertyName = "company")]
		public string Company { get; set; }
		[JsonProperty(PropertyName = "firstName")]
		public string FirstName { get; set; }
		[JsonProperty(PropertyName = "lastName")]
		public string LastName { get; set; }
		[JsonProperty(PropertyName = "country")]
		public string Country { get; set; }
		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }
		[JsonProperty(PropertyName = "phone")]
		public string Phone { get; set; }
		[JsonProperty(PropertyName = "website")]
		public string Website { get; set; }
		[JsonProperty(PropertyName = "key")]
		public string Key { get; set; }
	}
}
