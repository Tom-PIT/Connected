using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel.BigData;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	public interface ITransactionService
	{
		void Prepare(IPartitionConfiguration partition, JArray items);
		void Complete(Guid popReceipt);
		void Ping(Guid popReceipt);
		List<IQueueMessage> Dequeue(int count);
	}
}
