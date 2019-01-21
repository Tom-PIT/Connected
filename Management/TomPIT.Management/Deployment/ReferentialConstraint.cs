using Newtonsoft.Json;

namespace TomPIT.Deployment
{
	public class ReferentialConstraint
	{
		[JsonProperty(PropertyName = "schema")]
		public string Schema { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "uniqueSchema")]
		public string UniqueSchema { get; set; }
		[JsonProperty(PropertyName = "uniqueName")]
		public string UniqueName { get; set; }
		[JsonProperty(PropertyName = "matchOption")]
		public string MatchOption { get; set; }
		[JsonProperty(PropertyName = "updateRule")]
		public string UpdateRule { get; set; }
		[JsonProperty(PropertyName = "deleteRule")]
		public string DeleteRule { get; set; }
	}
}
