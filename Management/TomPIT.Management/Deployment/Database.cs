using Newtonsoft.Json;
using System.Collections.Generic;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.Deployment
{
	public class Database : IDatabase
	{
		[JsonProperty(PropertyName = "tables")]
		public List<ITable> Tables { get; set; }
		[JsonProperty(PropertyName = "views")]
		public List<IView> Views { get; set; }
		[JsonProperty(PropertyName = "routine")]
		public List<IRoutine> Routines { get; set; }
	}
}
