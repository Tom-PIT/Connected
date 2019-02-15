using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment
{
	public class Routine : SchemaBase, IRoutine
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		[JsonProperty(PropertyName = "definition")]
		public string Definition { get; set; }
	}
}
