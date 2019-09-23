using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Data;
using TomPIT.BigData.Persistence;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionService : SynchronizedClientRepository<IPartition, Guid>, IPartitionService
	{
		public PartitionService(ITenant tenant) : base(tenant, "partitions")
		{
			Files = new PartitionFilesCache(Tenant);
			Fields = new PartitionFieldStatisticsCache(Tenant);
		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryPartitions");
			var partitions = Tenant.Get<List<Partition>>(u);

			foreach (var partition in partitions)
				Set(partition.Configuration, partition, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "SelectPartition");
			var e = new JObject
			{
				{"configuration", id }
			};

			var partition = Tenant.Post<Partition>(u, e);

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

			var ms = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());

			var u = Instance.Tenant.CreateUrl("BigDataManagement", "InsertPartition");
			var e = new JObject
				{
					{"name", configuration.ComponentName() },
					{"configuration", configuration.Component },
					{ "status", PartitionStatus.Active.ToString() },
					{ "resourceGroup", ms.ResourceGroup }
				};

			Tenant.Post(u, e);
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
			var u = Tenant.CreateUrl("BigDataManagement", "InsertFile");
			var e = new JObject
			{
				{"partition", partition },
				{"node", node },
				{"key", key },
				{"timeStamp", timeStamp }
			};

			var id = Tenant.Post<Guid>(u, e);

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
			var u = Tenant.CreateUrl("BigDataManagement", "UpdateFile");
			var e = new JObject
			{
				{"token", file },
				{"startTimestamp", startTimeStamp },
				{"endTimestamp", endTimeStamp },
				{"count", count },
				{"status", status.ToString() }
			};

			Tenant.Post(u, e);

			Files.Notify(file);
		}

		public void DeleteFile(Guid file)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "DeleteFile");
			var e = new JObject
			{
				{"token", file }
			};

			Tenant.Post(u, e);

			Files.Notify(file, true);
		}

		public Guid LockFile(Guid file)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "LockFile");
			var e = new JObject
			{
				{"token", file }
			};

			return Tenant.Post<Guid>(u, e);
		}

		public void ReleaseFile(Guid unlockKey)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "UnlockFile");
			var e = new JObject
			{
				{"unlockKey", unlockKey }
			};

			Tenant.Post(u, e);
		}

		public void UpdateFileStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "UpdateFieldStatistics");
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

			Tenant.Post(u, e);

			Fields.Notify(file, fieldName);
		}

		public void ValidateSchema(Guid partition)
		{
			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(partition) as IPartitionConfiguration;
			var schema = Tenant.GetService<IPersistenceService>().SelectSchema(configuration);
			var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());
			var existingConfiguration = Tenant.GetService<IStorageService>().Query(microService.Token, BlobTypes.BigDataPartitionSchema, microService.ResourceGroup, partition.ToString());

			if (existingConfiguration == null || existingConfiguration.Count == 0)
			{
				CreateValidationSchema(microService, partition, schema);
				return;
			}
			else
			{
				var content = Instance.Tenant.GetService<IStorageService>().Download(existingConfiguration[0].Token);

				if (content == null || content.Content == null)
				{
					CreateValidationSchema(microService, partition, schema);
					return;
				}

				var existingSchema = SerializationExtensions.Deserialize<PartitionSchema>(Encoding.UTF8.GetString(content.Content));

				if (existingSchema.CompareTo(schema) != 0)
				{
					if (configuration.SchemaSynchronization == SchemaSynchronizationMode.Auto)
						UpdatePartition(partition, configuration.ComponentName(), PartitionStatus.Maintenance);
					else
						UpdatePartition(partition, configuration.ComponentName(), PartitionStatus.Invalid);

					throw new RuntimeException(SR.ErrBigDataPartitionSchemaChanged);
				}
			}
		}

		public void SaveSchemaImage(Guid partition)
		{
			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(partition) as IPartitionConfiguration;
			var schema = Tenant.GetService<IPersistenceService>().SelectSchema(configuration);
			var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());

			CreateValidationSchema(microService, partition, schema);
		}

		private void CreateValidationSchema(IMicroService microService, Guid partition, PartitionSchema schema)
		{
			Instance.Tenant.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = partition.ToString(),
				MicroService = microService.Token,
				PrimaryKey = partition.ToString(),
				ResourceGroup = microService.ResourceGroup,
				Type = BlobTypes.BigDataPartitionSchema
			}, Encoding.UTF8.GetBytes(SerializationExtensions.Serialize(schema)), StoragePolicy.Singleton);
		}

		public void UpdatePartition(Guid token, string name, PartitionStatus status)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "UpdatePartition");
			var e = new JObject
			{
				{"configuration", token },
				{"name", name },
				{"status", status.ToString() }
			};

			Instance.Tenant.Post(u, e);
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
