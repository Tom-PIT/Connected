using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	public class TableConstraint : SchemaBase, ITableConstraint
	{
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

	}
}
