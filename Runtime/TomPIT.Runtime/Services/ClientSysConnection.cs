using Newtonsoft.Json;

namespace TomPIT.Services
{
	internal class ClientSysConnection : IClientSysConnection
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
		[JsonProperty(PropertyName = "authenticationToken")]
		public string AuthenticationToken { get; set; }
	}
}
