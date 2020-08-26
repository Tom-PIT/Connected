using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TomPIT.BigData.Transactions
{
	internal interface IBufferingService
	{
		List<IPartitionBuffer> Dequeue(int count);
		void Enqueue(Guid partition, TimeSpan duration, JArray items);
		List<IPartitionBufferData> QueryData(Guid partition);
		void ClearData(Guid partition, TimeSpan nextVisible, long id);
	}
}
