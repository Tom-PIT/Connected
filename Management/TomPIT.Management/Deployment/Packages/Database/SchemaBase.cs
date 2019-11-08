using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	public abstract class SchemaBase : ISchema
	{
		[JsonProperty(PropertyName = "schema")]
		public string Schema { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
	}
}
