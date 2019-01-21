using Newtonsoft.Json;
using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public class Table
	{
		[JsonProperty(PropertyName = "schema")]
		public string Schema { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "columns")]
		public List<TableColumn> Columns { get; set; }
	}
}
