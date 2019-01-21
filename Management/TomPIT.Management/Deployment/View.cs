using Newtonsoft.Json;

namespace TomPIT.Deployment
{
	public class View
	{
		[JsonProperty(PropertyName = "schema")]
		public string Schema { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "definition")]
		public string Definition { get; set; }
	}
}
