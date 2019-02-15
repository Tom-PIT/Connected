using Newtonsoft.Json;
using System.Collections.Generic;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment
{
	public class PackageDatabase : IDatabase
	{
		[JsonProperty(PropertyName = "tables")]
		public List<ITable> Tables { get; set; }
		[JsonProperty(PropertyName = "views")]
		public List<IView> Views { get; set; }
		[JsonProperty(PropertyName = "routine")]
		public List<IRoutine> Routines { get; set; }
	}
}
