using Newtonsoft.Json;
using TomPIT.Serialization.Converters;

namespace TomPIT.Runtime.Configuration
{
	internal class ClientSysDeployment : IClientSysDeployment
	{
		[JsonProperty(PropertyName = "fileSystem")]
		[JsonConverter(typeof(DeploymentFileSystemConverter))]
		public IClientSysFileSystemDeployment FileSystem {get;set;}
	}
}
