using System;
using Newtonsoft.Json;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages
{
	internal class PackageConfigurationDatabase : IPackageConfigurationDatabase
	{
		[JsonProperty("connectionString")]
		public string ConnectionString { get; set; }
		[JsonProperty("connection")]
		public Guid Connection { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("dataProvider")]
		public string DataProvider { get; set; }
		[JsonProperty("dataProviderId")]
		public Guid DataProviderId { get; set; }
		[JsonProperty("enabled")]
		public bool Enabled { get; set; } = true;
	}
}
