using Newtonsoft.Json;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.Deployment
{
	public abstract class SchemaBase : ISchema
	{
		[JsonProperty(PropertyName = "schema")]
		public string Schema { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
	}
}
