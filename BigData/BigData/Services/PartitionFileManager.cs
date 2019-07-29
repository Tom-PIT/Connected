using System;

namespace TomPIT.BigData.Services
{
	internal class PartitionFileManager
	{
		public Guid CreateFile(Guid partition, string partitionKey, DateTime timestamp)
		{
			var node = Instance.GetService<INodeService>().SelectSmallest();

			if (node == null)
				throw new RuntimeException(SR.ErrBigDataNoNodes);

			IPartitionFile file = null;

			try
			{
				var fileId = Instance.GetService<IPartitionService>().InsertFile(partition, node.Token, partitionKey, timestamp);

				if (fileId == Guid.Empty)
					return Guid.Empty;

				file = Instance.GetService<IPartitionService>().SelectFile(fileId);

				Instance.GetService<IPersistenceService>().SynchronizeSchema(node, file);
				Instance.GetService<IPartitionService>().UpdateFile(file.FileName, file.StartTimestamp, file.EndTimestamp, file.Count, PartitionFileStatus.Open);

				return fileId;
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("BigData", ex.Source, ex.Message);

				if (file != null)
					TryRollbackFileCreate(file.FileName);
			}

			return Guid.Empty;
		}

		private void TryRollbackFileCreate(Guid file)
		{
			try
			{
				Instance.GetService<IPartitionService>().DeleteFile(file);
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("BigData", ex.Source, ex.Message);
			}
		}

		public Guid Lock(Guid file)
		{
			return Instance.GetService<IPartitionService>().LockFile(file);
		}

		public void Release(Guid file)
		{
			Instance.GetService<IPartitionService>().ReleaseFile(file);
		}
	}
}