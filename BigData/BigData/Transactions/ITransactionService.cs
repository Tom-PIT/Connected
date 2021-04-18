using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.BigData;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	public interface ITransactionService
	{
		void Prepare(IPartitionConfiguration partition, JArray items);
		void Complete(Guid popReceipt, Guid block);
		void Ping(Guid popReceipt, TimeSpan delay);
		ITransactionBlock Select(Guid token);
		List<IQueueMessage> Dequeue(int count);
		void CreateTransaction(IPartitionConfiguration partition, JArray items);
	}
}
