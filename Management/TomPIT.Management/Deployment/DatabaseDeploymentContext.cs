using System;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;
using TomPIT.Design.Serialization;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
	internal class DatabaseDeploymentContext : IDatabaseDeploymentContext
	{
		public DatabaseDeploymentContext(ISysConnection connection, IPackage package, string connectionString, IDatabase database)
		{
			Package = package;
			Connection = connection;
			ConnectionString = connectionString;
			Database = database;
		}

		private IPackage Package { get; }
		private ISysConnection Connection { get; }
		public string ConnectionString { get; }
		public IDatabase Database { get; }
		private Guid ResourceGroup => GetService<IResourceGroupService>().Default.Token;
		private string StateKey => ((IPackageDatabase)Database).Connection.ToString();

		public T GetService<T>()
		{
			return Connection.GetService<T>();
		}

		public IDatabase LoadState()
		{
			var blobs = Connection.GetService<IStorageService>().Query(Package.MicroService.Token, BlobTypes.DatabaseState, ResourceGroup, StateKey);

			if (blobs.Count == 0)
				return null;

			var content = Connection.GetService<IStorageService>().Download(blobs[0].Token);

			if (content == null)
				return null;

			return Connection.GetService<ISerializationService>().Deserialize(content.Content, typeof(PackageDatabase)) as IDatabase;
		}

		public void SaveState()
		{
			Connection.GetService<IStorageService>().Upload(new Blob
			{
				MicroService = Package.MicroService.Token,
				ContentType = "application/json",
				FileName = "databaseState.json",
				PrimaryKey = StateKey,
				ResourceGroup = ResourceGroup,
				Type = BlobTypes.DatabaseState
			}, Connection.GetService<ISerializationService>().Serialize(Database), StoragePolicy.Singleton);
		}
	}
}
