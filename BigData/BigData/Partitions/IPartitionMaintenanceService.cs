using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.BigData.Partitions
{
	internal interface IPartitionMaintenanceService
	{
		void Ping(Guid popReceipt);
		void Complete(Guid popReceipt, Guid partition);
		List<IQueueMessage> Dequeue(int count);
	}
}
