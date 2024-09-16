using System;
using System.Threading;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Persistence;
using TomPIT.Exceptions;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFileManager
	{
		public Guid CreateFile(Guid partition, Guid timezone, string partitionKey, DateTime timestamp)
		{
			var node = Tenant.GetService<INodeService>().SelectSmallest();

			if (node is null)
				throw new RuntimeException(SR.ErrBigDataNoNodes);

			IPartitionFile file = null;

			try
			{
				var fileId = Tenant.GetService<IPartitionService>().InsertFile(partition, node.Token, timezone, partitionKey, timestamp);

				if (fileId == Guid.Empty)
					return Guid.Empty;

				Tenant.GetService<IPartitionService>().NotifyFileChanged(fileId);
				file = Tenant.GetService<IPartitionService>().SelectFile(fileId);

				Tenant.GetService<IPersistenceService>().SynchronizeSchema(node, file);
				Tenant.GetService<IPartitionService>().UpdateFile(file.FileName, file.StartTimestamp, file.EndTimestamp, file.Count, PartitionFileStatus.Open);

				return fileId;
			}
			catch (Exception ex)
			{
				Tenant.LogError(ex.Source, ex.ToString(), "BigData");

				if (file != null)
					TryRollbackFileCreate(file.FileName);
			}

			return Guid.Empty;
		}

		private void TryRollbackFileCreate(Guid file)
		{
			try
			{
				Tenant.GetService<IPartitionService>().DeleteFile(file);
			}
			catch (Exception ex)
			{
				Tenant.LogError(ex.Source, ex.ToString(), "BigData");
			}
		}

		public Guid Lock(Guid file)
		{
			for (var i = 1; i < 10; i++)
			{
				var result = Tenant.GetService<IPartitionService>().LockFile(file);

				if (result != Guid.Empty)
					return result;

				Thread.Sleep(i * 50);
			}

			return Guid.Empty;
		}

		public void Release(Guid file)
		{
			Tenant.GetService<IPartitionService>().ReleaseFile(file);
		}
	}
}