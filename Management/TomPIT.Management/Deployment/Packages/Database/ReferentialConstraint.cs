using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	public class ReferentialConstraint : IReferentialConstraint
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "referenceSchema")]
		public string ReferenceSchema { get; set; }
		[JsonProperty(PropertyName = "referenceName")]
		public string ReferenceName { get; set; }
		[JsonProperty(PropertyName = "matchOption")]
		public string MatchOption { get; set; }
		[JsonProperty(PropertyName = "updateRule")]
		public string UpdateRule { get; set; }
		[JsonProperty(PropertyName = "deleteRule")]
		public string DeleteRule { get; set; }
	}
}
