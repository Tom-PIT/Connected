using Newtonsoft.Json;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.Deployment
{
	public class Routine : SchemaBase, IRoutine
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		[JsonProperty(PropertyName = "definition")]
		public string Definition { get; set; }
	}
}
