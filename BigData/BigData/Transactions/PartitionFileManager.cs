using Amt.Core.Diagnostics;
using Amt.DataHub.Partitions;
using Amt.DataHub.Schemas;
using Amt.Sdk.DataHub;
using Amt.Sdk.Exceptions;
using Amt.Sys.Model.DataHub;
using System;

namespace Amt.DataHub.Transactions
{
	internal class PartitionFileManager
	{
		public long CreateFile(int partitionId, string key, DateTime timestamp, bool force = false)
		{
			var node = AmtShell.GetService<INodeService>().SelectSmallest();

			if (node == null)
				throw new ApiException("No active nodes found");

			PartitionFile file = null;

			try
			{
				var fileId = PartitionModel.InsertPartitionFile(partitionId, node.Id, key, timestamp, force);

				if (fileId == 0)
					return 0;

				file = PartitionModel.SelectFile(fileId);

				var schema = new DatabaseSchema(node, file);

				schema.Update();

				PartitionModel.ChangeFileStatus(file.Id, PartitionFileStatus.Open);

				return fileId;
			}
			catch (Exception ex)
			{
				AmtShell.GetService<ILoggingService>().Error("Data hub", "CreateFile", ex);

				if (file != null)
					TryRollbackFileCreate(file.Id);
			}

			return 0;
		}

		private void TryRollbackFileCreate(long id)
		{
			try
			{
				PartitionModel.DeletePartitionFile(id);
			}
			catch (Exception ex)
			{
				AmtShell.GetService<ILoggingService>().Error("Data hub", "TryRollbackFileCreate", ex);
			}
		}

		public PartitionFile Lock(long id)
		{
			return PartitionModel.LockFile(id);
		}

		public void Release(long fileId)
		{
			PartitionModel.ReleaseLock(fileId);
		}
	}
}