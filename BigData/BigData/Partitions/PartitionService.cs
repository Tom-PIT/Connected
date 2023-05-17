using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
		private const string DefaultController = "BigDataManagement";
		public PartitionService(ITenant tenant) : base(tenant, "partitions")
		{
			Files = new PartitionFilesCache(Tenant);
			Fields = new PartitionFieldStatisticsCache(Tenant);
		}

		private PartitionFilesCache Files { get; }
		private PartitionFieldStatisticsCache Fields { get; }

		protected override void OnInitializing()
		{
			foreach (var partition in Instance.SysProxy.Management.BigData.QueryPartitions())
				Set(partition.Configuration, partition, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Instance.SysProxy.Management.BigData.SelectPartition(id) is IPartition partition)
				Set(id, partition, TimeSpan.Zero);
		}
		public ImmutableList<IPartition> Query()
		{
			return All();
		}

		public IPartition Select(IPartitionConfiguration configuration)
		{
			var r = Get(f => f.Configuration == configuration.Component);

			if (r is not null)
				return r;

			var ms = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());

			Instance.SysProxy.Management.BigData.InsertPartition(configuration.Component, configuration.ComponentName(), PartitionStatus.Active, ms.ResourceGroup);
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

		public Guid InsertFile(Guid partition, Guid node, Guid timezone, string key, DateTime timeStamp)
		{
			var id = Instance.SysProxy.Management.BigData.InsertFile(partition, node, key, timeStamp, timezone);

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

		public ImmutableList<IPartitionFile> QueryFiles(Guid partition, Guid timezone, string key, DateTime startTimestamp, DateTime endTimestamp)
		{
			return Files.Query(partition, timezone, key, startTimestamp, endTimestamp);
		}

		public void UpdateFile(Guid file, DateTime startTimeStamp, DateTime endTimeStamp, int count, PartitionFileStatus status)
		{
			Instance.SysProxy.Management.BigData.UpdateFile(file, startTimeStamp, endTimeStamp, count, status);
			Files.Notify(file);
		}

		public void DeleteFile(Guid file)
		{
			Instance.SysProxy.Management.BigData.DeleteFile(file);
			Files.Notify(file, true);
		}

		public Guid LockFile(Guid file)
		{
			return Instance.SysProxy.Management.BigData.LockFile(file);
		}

		public void ReleaseFile(Guid unlockKey)
		{
			Instance.SysProxy.Management.BigData.UnlockFile(unlockKey);
		}

		public void UpdateFileStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			Instance.SysProxy.Management.BigData.UpdateFieldStatistics(file, fieldName, startString, endString, startNumber, endNumber, startDate, endDate);
			Fields.Notify(file, fieldName);
		}

		public void ValidateSchema(Guid partition)
		{
			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(partition) as IPartitionConfiguration;
			var schema = Tenant.GetService<IPersistenceService>().SelectSchema(configuration);
			var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());
			var existingConfiguration = Tenant.GetService<IStorageService>().Download(microService.Token, BlobTypes.BigDataPartitionSchema, microService.ResourceGroup, partition.ToString());

			if (existingConfiguration is null || existingConfiguration.Content is null)
			{
				CreateValidationSchema(microService, partition, schema);
				return;
			}

			var existingSchema = Serializer.Deserialize<PartitionSchema>(Encoding.UTF8.GetString(existingConfiguration.Content));

			if (existingSchema.CompareTo(schema) != 0)
			{
				var p = Select(configuration);

				if (p.Status == PartitionStatus.Active)
				{
					if (configuration.SchemaSynchronization == SchemaSynchronizationMode.Auto)
						UpdatePartition(partition, configuration.ComponentName(), PartitionStatus.Maintenance);
					else
						UpdatePartition(partition, configuration.ComponentName(), PartitionStatus.Invalid);
				}

				throw new RuntimeException(SR.ErrBigDataPartitionSchemaChanged);
			}
		}

		public void SaveSchemaImage(Guid partition)
		{
			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(partition) as IPartitionConfiguration;
			var schema = Tenant.GetService<IPersistenceService>().SelectSchema(configuration);
			var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());

			CreateValidationSchema(microService, partition, schema);
		}

		private static void CreateValidationSchema(IMicroService microService, Guid partition, PartitionSchema schema)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = partition.ToString(),
				MicroService = microService.Token,
				PrimaryKey = partition.ToString(),
				ResourceGroup = microService.ResourceGroup,
				Type = BlobTypes.BigDataPartitionSchema
			}, Encoding.UTF8.GetBytes(Serializer.Serialize(schema)), StoragePolicy.Singleton);
		}

		public void UpdatePartition(Guid token, string name, PartitionStatus status)
		{
			Instance.SysProxy.Management.BigData.UpdatePartition(token, name, status);
			Refresh(token);
		}

		public ImmutableList<IPartitionFile> QueryFiles(Guid partition)
		{
			return Files.Query(partition);
		}

		public ImmutableList<IPartitionFile> QueryFiles(Guid partition, Guid timezone, string key, DateTime startTimestamp, DateTime endTimestamp, List<IndexParameter> parameters)
		{
			var candidates = Files.Query(partition, timezone, key, startTimestamp, endTimestamp);
			var result = candidates.GroupBy(f => f.FileName).Select(f => f.First()).Select(f => f.FileName).ToList();
			var fieldHits = new List<Guid>();

			if (parameters.Count == 0)
				return candidates;

			foreach (var parameter in parameters)
			{
				List<Guid> hits = null;

				if (parameter is IndexDiscreteParameter discrete)
					hits = Fields.Query(result, partition, key, discrete.Name, discrete.ValueType, discrete.Value);
				else if (parameter is IndexRangeParameter range)
					hits = Fields.Query(result, partition, key, range.Name, range.ValueType, range.StartValue, range.EndValue);
				else if (parameter is IndexArrayParameter array)
					hits = Fields.Query(result, partition, key, array.Name, array.ValueType, array.Values);

				foreach (var hit in hits)
				{
					if (!fieldHits.Contains(hit))
						fieldHits.Add(hit);
				}
			}

			if (fieldHits.Count == 0)
				return ImmutableList<IPartitionFile>.Empty;

			return Files.Where(fieldHits);
		}
	}
}
