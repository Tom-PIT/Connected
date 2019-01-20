using Newtonsoft.Json;

namespace TomPIT.Sys.Services
{
	internal class ServerSysJwToken : IServerSysJwToken
	{
		[JsonProperty(PropertyName = "validIssuer")]
		public string ValidIssuer { get; set; }
		[JsonProperty(PropertyName = "validAudience")]
		public string ValidAudience { get; set; }
		[JsonProperty(PropertyName = "issuerSigningKey")]
		public string IssuerSigningKey { get; set; }
	}
}
