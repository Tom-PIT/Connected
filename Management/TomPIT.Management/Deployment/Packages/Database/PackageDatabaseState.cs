using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	internal class PackageDatabaseState
	{
		public string ConnectionString { get; set; }
		public IDatabase Database { get; set; }
	}
}
