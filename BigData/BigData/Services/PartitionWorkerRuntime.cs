using Amt.DataHub.Data;
using Amt.DataHub.Partitions;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;

namespace Amt.DataHub
{
	public class PartitionWorkerRuntime
	{
		public bool ProcessData(PartitionWorker worker, TransactionTask task)
		{
			if (worker == null || task == null)
				return true;

			var block = DataHubProxy.ProcessTask(task.Identifier, 30);

			if (block == null)
				return true;

			var config = AmtShell.GetService<IConfigurationService>().Select<PartitionConfiguration>(worker.Partition);

			if (config == null)
			{
				Log.Warning(this, "Partition not found.", LogEvents.DhPartitionNull, worker.Partition.AsString());
				return true;
			}

			return PartitionModel.Update(config.RefId, task.Id, block, task.PopReceipt);
		}
	}
}