using Newtonsoft.Json;
using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public class Database
	{
		[JsonProperty(PropertyName = "tables")]
		List<Table> Tables { get; set; }
		[JsonProperty(PropertyName = "views")]
		List<View> Views { get; set; }
		[JsonProperty(PropertyName = "references")]
		List<ReferentialConstraint> References { get; set; }
		[JsonProperty(PropertyName = "constraints")]
		List<TableConstraint> Constraints { get; set; }
		[JsonProperty(PropertyName = "routine")]
		List<Routine> Routines { get; set; }
		[JsonProperty(PropertyName = "columnUsage")]
		List<ConstraintColumnUsage> ColumnUsage { get; set; }
	}
}
