using Newtonsoft.Json;

namespace TomPIT.Sys.Services
{
	internal class ServerSysConnectionStrings : IServerSysConnectionStrings
	{
		[JsonProperty(PropertyName = "sys")]
		public string Sys { get; set; }
	}
}
