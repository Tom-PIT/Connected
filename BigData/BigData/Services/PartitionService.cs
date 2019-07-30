using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Data;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	internal class PartitionService : SynchronizedClientRepository<IPartition, Guid>, IPartitionService
	{
		public PartitionService(ISysConnection connection) : base(connection, "partitions")
		{
			Files = new PartitionFilesCache(Connection);
			Fields = new PartitionFieldStatisticsCache(Connection);
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryPartitions");
			var partitions = Connection.Get<List<Partition>>(u);

			foreach(var partition in partitions)
				Set(partition.Configuration, partition, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectPartition");
			var e = new JObject
			{
				{"configuration", id }
			};

			var partition = Connection.Post<Partition>(u, e);

			if (partition != null)
				Set(id, partition, TimeSpan.Zero);
		}
		public List<IPartition> Query()
		{
			return All();
		}

		public IPartition Select(IPartitionConfiguration configuration)
		{
			var r = Get(f => f.Configuration == configuration.Component);

			if (r != null)
				return r;

			var ms = Connection.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService(Connection));

			var u = Instance.Connection.CreateUrl("BigDataManagement", "InsertPartition");
			var e = new JObject
				{
					{"name", configuration.ComponentName(Connection) },
					{"configuration", configuration.Component },
					{ "status", PartitionStatus.Active.ToString() },
					{ "resourceGroup", ms.ResourceGroup }
				};

			Connection.Post(u, e);
			Refresh(configuration.Component);

			return Get(f => f.Configuration == configuration.Component);
		}

		public IPartitionFile SelectFile(Guid fileName)
		{
			return Files.Select(fileName);
		}

		public void NotifyChanged(Guid token)
		{
			Refresh(token);
		}

		public void NotifyRemoved(Guid token)
		{
			Remove(token);
		}

		public Guid InsertFile(Guid partition, Guid node, string key, DateTime timeStamp)
		{
			var u = Connection.CreateUrl("BigDataManagement", "InsertFile");
			var e = new JObject
			{
				{"partition", partition },
				{"node", node },
				{"key", key },
				{"timeStamp", timeStamp }
			};

			var id = Connection.Post<Guid>(u, e);

			Files.Notify(id);

			return id;
		}

		public void NotifyFileChanged(Guid token)
		{
			Files.Notify(token);
		}

		public void NotifyFileRemoved(Guid token)
		{
			Files.Notify(token, true);
		}

		public void NotifyFieldStatisticChanged(Guid file, string fieldName)
		{
			Fields.Notify(file, fieldName);
		}

		public List<IPartitionFile> QueryFiles(Guid partition, string key, DateTime startTimestamp, DateTime endTimestamp)
		{
			return Files.Query(partition, key, startTimestamp, endTimestamp);
		}

		public void UpdateFile(Guid file, DateTime startTimeStamp, DateTime endTimeStamp, int count, PartitionFileStatus status)
		{
			var u = Connection.CreateUrl("BigDataManagement", "UpdateFile");
			var e = new JObject
			{
				{"token", file },
				{"startTimestamp", startTimeStamp },
				{"endTimestamp", endTimeStamp },
				{"count", count },
				{"status", status.ToString() }
			};

			Connection.Post(u, e);

			Files.Notify(file);
		}

		public void DeleteFile(Guid file)
		{
			var u = Connection.CreateUrl("BigDataManagement", "DeleteFile");
			var e = new JObject
			{
				{"token", file }
			};

			Connection.Post(u, e);

			Files.Notify(file, true);
		}

		public Guid LockFile(Guid file)
		{
			var u = Connection.CreateUrl("BigDataManagement", "LockFile");
			var e = new JObject
			{
				{"token", file }
			};

			return Connection.Post<Guid>(u, e);
		}

		public void ReleaseFile(Guid unlockKey)
		{
			var u = Connection.CreateUrl("BigDataManagement", "UnlockFile");
			var e = new JObject
			{
				{"unlockKey", unlockKey }
			};

			Connection.Post(u, e);
		}

		public void UpdateFileStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			var u = Connection.CreateUrl("BigDataManagement", "UpdateFieldStatistics");
			var e = new JObject
			{
				{"file", file },
				{"fieldName", fieldName },
				{"startString", startString },
				{"endString", endString },
				{"startNumber", startNumber },
				{"endNumber", endNumber },
				{"startDate", startDate },
				{"endDate", endDate }
			};

			Connection.Post(u, e);

			Fields.Notify(file, fieldName);
		}

		public void ValidateSchema(Guid partition)
		{
			var configuration = Connection.GetService<IComponentService>().SelectConfiguration(partition) as IPartitionConfiguration;
			var schema = Connection.GetService<IPersistenceService>().SelectSchema(configuration);
			var microService = Connection.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService(Connection));
			var existingConfiguration = Connection.GetService<IStorageService>().Query(microService.Token, BlobTypes.BigDataPartitionSchema, microService.ResourceGroup, partition.ToString());

			if (existingConfiguration == null || existingConfiguration.Count == 0)
			{
				CreateValidationSchema(microService, partition, schema);
				return;
			}
			else
			{
				var content = Instance.GetService<IStorageService>().Download(existingConfiguration[0].Token);

				if (content == null || content.Content == null)
				{
					CreateValidationSchema(microService, partition, schema);
					return;
				}

				var existingSchema = Types.Deserialize<PartitionSchema>(Encoding.UTF8.GetString(content.Content));

				if (existingSchema.CompareTo(schema) != 0)
				{
					if (configuration.SchemaSynchronization == SchemaSynchronizationMode.Auto)
						UpdatePartition(partition, configuration.ComponentName(Connection), PartitionStatus.Maintenance);
					else
						UpdatePartition(partition, configuration.ComponentName(Connection), PartitionStatus.Invalid);

					throw new RuntimeException(SR.ErrBigDataPartitionSchemaChanged);
				}
			}
		}

		public void SaveSchemaImage(Guid partition)
		{
			var configuration = Connection.GetService<IComponentService>().SelectConfiguration(partition) as IPartitionConfiguration;
			var schema = Connection.GetService<IPersistenceService>().SelectSchema(configuration);
			var microService = Connection.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService(Connection));

			CreateValidationSchema(microService, partition, schema);
		}

		private void CreateValidationSchema(IMicroService microService, Guid partition, PartitionSchema schema)
		{
			Instance.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = partition.ToString(),
				MicroService = microService.Token,
				PrimaryKey = partition.ToString(),
				ResourceGroup = microService.ResourceGroup,
				Type = BlobTypes.BigDataPartitionSchema
			}, Encoding.UTF8.GetBytes(Types.Serialize(schema)), StoragePolicy.Singleton);
		}

		public void UpdatePartition(Guid token, string name, PartitionStatus status)
		{
			var u = Connection.CreateUrl("BigDataManagement", "UpdatePartition");
			var e = new JObject
			{
				{"configuration", token },
				{"name", name },
				{"status", status.ToString() }
			};

			Instance.Connection.Post(u, e);
			Refresh(token);
		}

		public List<IPartitionFile> QueryFiles(Guid partition)
		{
			return Files.Query(partition);
		}

		private PartitionFilesCache Files { get; }
		private PartitionFieldStatisticsCache Fields { get; }
	}
}
