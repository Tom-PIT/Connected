using System;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;
using TomPIT.Design.Serialization;
using TomPIT.Environment;
using TomPIT.Management.Deployment.Packages.Database;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
	internal class DatabaseDeploymentContext : TenantObject, IDatabaseDeploymentContext
	{
		public DatabaseDeploymentContext(ITenant tenant, IPackage package, string connectionString, IDatabase database) : base(tenant)
		{
			Package = package;
			ConnectionString = connectionString;
			Database = database;
		}

		private IPackage Package { get; }
		public string ConnectionString { get; }
		public IDatabase Database { get; }
		private Guid ResourceGroup => Tenant.GetService<IResourceGroupService>().Default.Token;
		private string StateKey => ((IPackageDatabase)Database).Connection.ToString();

		public IDatabase LoadState()
		{
			var blobs = Tenant.GetService<IStorageService>().Query(Package.MicroService.Token, BlobTypes.DatabaseState, ResourceGroup, StateKey);

			if (blobs.Count == 0)
				return null;

			var content = Tenant.GetService<IStorageService>().Download(blobs[0].Token);

			if (content == null)
				return null;

			return Tenant.GetService<ISerializationService>().Deserialize(content.Content, typeof(PackageDatabase)) as IDatabase;
		}

		public void SaveState()
		{
			Tenant.GetService<IStorageService>().Upload(new Blob
			{
				MicroService = Package.MicroService.Token,
				ContentType = "application/json",
				FileName = "databaseState.json",
				PrimaryKey = StateKey,
				ResourceGroup = ResourceGroup,
				Type = BlobTypes.DatabaseState
			}, Tenant.GetService<ISerializationService>().Serialize(Database), StoragePolicy.Singleton);
		}

		public T GetService<T>()
		{
			return Tenant.GetService<T>();
		}
	}
}
