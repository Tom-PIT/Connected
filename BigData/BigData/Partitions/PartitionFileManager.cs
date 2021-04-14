using System;
using System.Threading;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Persistence;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFileManager
	{
		public Guid CreateFile(Guid partition, string partitionKey, DateTime timestamp)
		{
			var node = MiddlewareDescriptor.Current.Tenant.GetService<INodeService>().SelectSmallest();

			if (node == null)
				throw new RuntimeException(SR.ErrBigDataNoNodes);

			IPartitionFile file = null;

			try
			{
				var fileId = MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().InsertFile(partition, node.Token, partitionKey, timestamp);

				if (fileId == Guid.Empty)
					return Guid.Empty;

				file = MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().SelectFile(fileId);

				MiddlewareDescriptor.Current.Tenant.GetService<IPersistenceService>().SynchronizeSchema(node, file);
				MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().UpdateFile(file.FileName, file.StartTimestamp, file.EndTimestamp, file.Count, PartitionFileStatus.Open);

				return fileId;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, "BigData");

				if (file != null)
					TryRollbackFileCreate(file.FileName);
			}

			return Guid.Empty;
		}

		private void TryRollbackFileCreate(Guid file)
		{
			try
			{
				MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().DeleteFile(file);
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, "BigData");
			}
		}

		public Guid Lock(Guid file)
		{
			for (var i = 1; i < 10; i++)
			{
				var result = MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().LockFile(file);

				if (result != Guid.Empty)
					return result;

				Thread.Sleep(i * 50);
			}

			return Guid.Empty;
		}

		public void Release(Guid file)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().ReleaseFile(file);
		}
	}
}