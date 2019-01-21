using Newtonsoft.Json;

namespace TomPIT.Deployment
{
	public class TableConstraint
	{
		[JsonProperty(PropertyName = "schema")]
		public string Schema { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "tableSchema")]
		public string TableSchema { get; set; }
		[JsonProperty(PropertyName = "tableName")]
		public string TableName { get; set; }
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

	}
}
