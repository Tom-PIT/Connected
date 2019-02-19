using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment
{
	public class PackageDatabase : IPackageDatabase
	{
		[JsonProperty(PropertyName = "tables")]
		public List<ITable> Tables { get; set; }
		[JsonProperty(PropertyName = "views")]
		public List<IView> Views { get; set; }
		[JsonProperty(PropertyName = "routine")]
		public List<IRoutine> Routines { get; set; }
		[JsonProperty(PropertyName = "connection")]
		public Guid Connection { get; set; }
		[JsonProperty(PropertyName = "dataProviderId")]
		public Guid DataProviderId { get; set; }
		[JsonProperty(PropertyName = "dataProvider")]
		public string DataProvider { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
	}
}
