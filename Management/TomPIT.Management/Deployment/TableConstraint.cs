using Newtonsoft.Json;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.Deployment
{
	public class TableConstraint : SchemaBase, ITableConstraint
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

	}
}
