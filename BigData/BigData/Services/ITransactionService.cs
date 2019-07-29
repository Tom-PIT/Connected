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
		void Complete(Guid popReceipt, Guid block);
		void Ping(Guid popReceipt);
		ITransactionBlock Select(Guid token);
		List<IQueueMessage> Dequeue(int count);
		
	}
}
