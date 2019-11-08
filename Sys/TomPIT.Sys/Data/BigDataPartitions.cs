using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;
using TomPIT.Threading;

namespace TomPIT.Sys.Data
{
	internal class BigDataPartitions : SynchronizedRepository<IPartition, Guid>
	{
		private const string MaintenanceQueue = "bigdatapartition";

		private Lazy<LockerContainer<Guid>> _locker = new Lazy<Threading.LockerContainer<Guid>>();
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

		public void Update(Guid configuration, string name, PartitionStatus status)
		{
			var partition = Select(configuration);

			if (partition == null)
				throw new SysException(SR.ErrBigDataPartitionNotFound);

			var locker = RequestLock(partition);

			lock (locker.Sync)
			{
				partition = Select(configuration);
				var maintenance = status == PartitionStatus.Maintenance && partition.Status != PartitionStatus.Maintenance;

				Update(configuration, name, status, partition.FileCount);

				if (maintenance)
					Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(MaintenanceQueue, partition.Configuration.ToString(), TimeSpan.FromDays(7), TimeSpan.Zero, SysDb.Messaging.QueueScope.System);
			}
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

		private LockerContainer<Guid> Locker => _locker.Value;

		public Locker RequestLock(IPartition partition)
		{
			return Locker.Request(partition.Configuration);
		}

		public List<IQueueMessage> DequeueMaintenance(int count, TimeSpan nextVisible)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(MaintenanceQueue, count, nextVisible);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			var partition = new Guid(m.Message);

			var p = Select(partition);

			if (p != null)
			{
				var locker = RequestLock(p);
				lock (locker)
				{
					p = Select(partition);
					Update(p.Configuration, p.Name, PartitionStatus.Active);
				}
			}

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
		}
	}
}