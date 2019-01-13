using Amt.Sdk.DataHub;
using System;
using System.Data;

namespace Amt.DataHub.Data
{
	public static class DataHubProxy
	{
		public static DataTable ProcessTask(Guid taskId, int timeout)
		{
			var task = DataHubModel.SelectTransactionTask(taskId);

			if (task == null)
			{
				Log.Warning(typeof(DataHubProxy), "Transaction task not found.", LogEvents.DhTransactionTaskNull, taskId.AsString());
				return null;
			}

			/*
			 * comment: transaction is already queued. no need to lock it
			 */

			//if (!DataHubModel.StartTransactionTask(task.Id, timeout))
			//{
			//	Log.Warning(typeof(DataHubProxy), "Transaction task could not be started.", LogEvents.DhTransactionTaskStartError, task.Id.AsString());
			//	return null;
			//}

			var block = DataHubModel.SelectTransactionBlock(task.BlockId);

			if (block == null)
			{
				Log.Warning(typeof(DataHubProxy), "Transaction block not found.", LogEvents.DhTransactionBlobkNull, task.Id.AsString(), task.BlockId.AsString());
				return null;
			}

			return Storage.LoadBlock(block.Identifier);
		}

		public static void CancelTask(Guid popReceipt, string connectionId)
		{
			var task = DataHubModel.SelectTransactionTaskByPopReceipt(popReceipt);

			if (task == null || task.Status == TransactionTaskStatus.Idle)
				return;

			DataHubModel.CancelTransactionTask(task.PopReceipt, connectionId);
		}

		public static void CompleteTask(Guid popReceipt)
		{
			DataHubModel.CompleteTask(popReceipt);
		}
	}
}
