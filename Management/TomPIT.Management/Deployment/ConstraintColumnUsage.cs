using Newtonsoft.Json;

namespace TomPIT.Deployment
{
	public class ConstraintColumnUsage
	{
		[JsonProperty(PropertyName = "tableSchema")]
		public string TableSchema { get; set; }
		[JsonProperty(PropertyName = "tableName")]
		public string TableName { get; set; }
		[JsonProperty(PropertyName = "column")]
		public string Column { get; set; }
		[JsonProperty(PropertyName = "constraintSchema")]
		public string ConstraintSchema { get; set; }
		[JsonProperty(PropertyName = "constraintName")]
		public string ConstraintName { get; set; }
	}
}
