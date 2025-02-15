﻿using System;
using System.Collections.Immutable;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.BigData
{
	public class BigDataPartitionFilesModel : SynchronizedRepository<IPartitionFile, Guid>
	{
		public BigDataPartitionFilesModel(IMemoryCache container) : base(container, "bigdatapartitionfiles")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.QueryFiles();

			foreach (var i in ds)
				Set(i.FileName, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.SelectFile(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IPartitionFile Select(Guid token)
		{
			return Get(token);
		}

		public Guid Lock(Guid token)
		{
			var file = Select(token);

			if (file == null)
				throw new SysException(SR.ErrBigDataFileNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.LockFile(file);
		}

		public void Unlock(Guid unlockKey)
		{
			Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.UnlockFile(unlockKey);
		}

		public ImmutableList<IPartitionFile> Query(Guid partition)
		{
			return Where(f => f.Partition == partition);
		}

		public ImmutableList<IPartitionFile> Query()
		{
			return All();
		}

		public Guid Insert(Guid partition, Guid node, Guid timezone, string key, DateTime timestamp)
		{
			var p = DataModel.BigDataPartitions.Select(partition);

			if (p == null)
				throw new SysException(SR.ErrBigDataPartitionNotFound);

			if (p.Status == PartitionStatus.Maintenance)
				throw new SysException(SR.ErrBigDataPartitionReadOnly);

			var n = DataModel.BigDataNodes.Select(node);

			if (n == null)
				throw new SysException(SR.ErrBigDataNodeNotFound);

			ITimeZone tz = null;

			if (timezone != Guid.Empty)
			{
				tz = DataModel.BigDataTimeZones.Select(timezone);

				if (tz is null)
					throw new SysException(SR.ErrBigDataTimezoneNotFound);
			}

			var file = Guid.NewGuid();
			var locker = DataModel.BigDataPartitions.RequestLock(p);

			lock (locker.Sync)
			{
				p = DataModel.BigDataPartitions.Select(partition);

				Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.InsertFile(p, n, tz, key, timestamp, file, PartitionFileStatus.Creating);
				Refresh(file);

				DataModel.BigDataPartitions.Update(p.Configuration, p.Name, p.Status, Count);
			}

			BigDataNotifications.PartitionFileAdded(file);

			return file;
		}

		public void Update(Guid token, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status)
		{
			var file = Select(token);

			if (file == null)
				throw new SysException(SR.ErrBigDataFileNotFound);

			var partition = DataModel.BigDataPartitions.Select(file.Partition);
			var locker = DataModel.BigDataPartitions.RequestLock(partition);

			lock (locker.Sync)
			{
				partition = DataModel.BigDataPartitions.Select(file.Partition);

				if (partition.Status == PartitionStatus.Maintenance)
					throw new SysException(SR.ErrBigDataPartitionReadOnly);

				Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.UpdateFile(file, startTimestamp, endTimestamp, count, status);
			}

			Refresh(file.FileName);
			BigDataNotifications.PartitionFileChanged(file.FileName);
		}

		public void Delete(Guid token)
		{
			var file = Select(token);

			if (file == null)
				throw new SysException(SR.ErrBigDataFileNotFound);

			var partition = DataModel.BigDataPartitions.Select(file.Partition);
			var locker = DataModel.BigDataPartitions.RequestLock(partition);

			lock (locker.Sync)
			{
				partition = DataModel.BigDataPartitions.Select(file.Partition);

				if (partition.Status == PartitionStatus.Maintenance)
					throw new SysException(SR.ErrBigDataPartitionReadOnly);

				Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.DeleteFile(file);
				Remove(file.FileName);
				Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.Update(partition, partition.Name, partition.Status, Count);
			}

			BigDataNotifications.PartitionFileRemoved(file.FileName);
		}
	}
}
