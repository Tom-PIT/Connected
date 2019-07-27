using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class BigDataPartitions : SynchronizedRepository<IPartition, Guid>
	{
		public BigDataPartitions(IMemoryCache container) : base(container, "bigdatapartitions")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.Query();

			foreach (var i in ds)
				Set(i.Configuration, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IPartition Select(Guid token)
		{
			return Get(token);
		}

		public List<IPartition> Query()
		{
			return All();
		}

		public void Insert(Guid configuration, string name, PartitionStatus status, Guid resourceGroup)
		{
			var rg = DataModel.ResourceGroups.Select(resourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.Insert(rg, configuration, name, status, DateTime.UtcNow);

			Refresh(configuration);
			BigDataNotifications.PartitionAdded(configuration);
		}

		public void Update(Guid configuration, string name, PartitionStatus status, int fileCount)
		{
			var partition = Select(configuration);

			if (partition == null)
				throw new SysException(SR.ErrBigDataPartitionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.Update(partition, name, status, fileCount);

			Refresh(partition.Configuration);
			BigDataNotifications.PartitionChanged(partition.Configuration);
		}

		public void Delete(Guid configuration)
		{
			var partition = Select(configuration);

			if (partition == null)
				throw new SysException(SR.ErrBigDataPartitionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.Delete(partition);

			Refresh(partition.Configuration);
			BigDataNotifications.PartitionRemoved(partition.Configuration);
		}
	}
}
