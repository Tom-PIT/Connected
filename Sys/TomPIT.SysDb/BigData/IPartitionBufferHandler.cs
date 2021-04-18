using System;
using System.Collections.Generic;
using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface IPartitionBufferHandler
	{
		List<IPartitionBuffer> Dequeue(int count, DateTime date, DateTime nextVisible);
		List<IPartitionBufferData> QueryData(IPartitionBuffer buffer);
		List<IPartitionBuffer> Query();
		void Update(IPartitionBuffer buffer, DateTime nextVisible);
		void Insert(Guid partition, DateTime nextVisible);
		IPartitionBuffer Select(Guid partition);
		void Clear(IPartitionBuffer buffer, long id);
		void InsertData(IPartitionBuffer buffer, byte[] data);
	}
}
