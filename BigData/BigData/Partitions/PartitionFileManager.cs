using System;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Persistence;
using TomPIT.Diagostics;
using TomPIT.Exceptions;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFileManager
	{
		public Guid CreateFile(Guid partition, string partitionKey, DateTime timestamp)
		{
			var node = Instance.Tenant.GetService<INodeService>().SelectSmallest();

			if (node == null)
				throw new RuntimeException(SR.ErrBigDataNoNodes);

			IPartitionFile file = null;

			try
			{
				var fileId = Instance.Tenant.GetService<IPartitionService>().InsertFile(partition, node.Token, partitionKey, timestamp);

				if (fileId == Guid.Empty)
					return Guid.Empty;

				file = Instance.Tenant.GetService<IPartitionService>().SelectFile(fileId);

				Instance.Tenant.GetService<IPersistenceService>().SynchronizeSchema(node, file);
				Instance.Tenant.GetService<IPartitionService>().UpdateFile(file.FileName, file.StartTimestamp, file.EndTimestamp, file.Count, PartitionFileStatus.Open);

				return fileId;
			}
			catch (Exception ex)
			{
				Instance.Tenant.LogError("BigData", ex.Source, ex.Message);

				if (file != null)
					TryRollbackFileCreate(file.FileName);
			}

			return Guid.Empty;
		}

		private void TryRollbackFileCreate(Guid file)
		{
			try
			{
				Instance.Tenant.GetService<IPartitionService>().DeleteFile(file);
			}
			catch (Exception ex)
			{
				Instance.Tenant.LogError("BigData", ex.Source, ex.Message);
			}
		}

		public Guid Lock(Guid file)
		{
			return Instance.Tenant.GetService<IPartitionService>().LockFile(file);
		}

		public void Release(Guid file)
		{
			Instance.Tenant.GetService<IPartitionService>().ReleaseFile(file);
		}
	}
}