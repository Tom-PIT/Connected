using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	internal interface IPartitionMaintenanceService
	{
		void Ping(Guid popReceipt);
		void Complete(Guid popReceipt, Guid partition);
		List<IQueueMessage> Dequeue(int count);
	}
}
