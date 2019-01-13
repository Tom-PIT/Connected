using Amt.DataHub.Partitions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Models;
using System;
using System.Data;

namespace Amt.DataHub.Transactions
{
	public class ProcessArgs : InProcessArgs
	{
		public Guid TaskPopReceipt { get; set; }

		public ProcessArgs(IApplicationModel model, long taskId, DataTable data, Guid taskPopReceipt) : base(model, taskId, data)
		{
			TaskPopReceipt = taskPopReceipt;
		}

		public override void SendData(Guid partition, DataTable data)
		{
			var config = AmtShell.GetService<IConfigurationService>().Select<PartitionConfiguration>(partition);

			if (config == null)
			{
				Amt.Log.Warning(this, "Partition not found.", LogEvents.DhPartitionNull, partition.AsString());
				Cancel = true;
				return;
			}

			PartitionModel.Update(partition, TaskId, data, TaskPopReceipt);
		}
	}
}