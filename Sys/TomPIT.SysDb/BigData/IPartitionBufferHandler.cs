using System.Collections.Generic;
using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface IPartitionBufferHandler
	{
		void Update(List<IPartitionBuffer> buffers);
		List<IPartitionBuffer> Query();
		List<IPartitionBufferData> QueryData(IPartitionBuffer buffer);
		void Clear(IPartitionBuffer buffer, long id);
		void InsertData(IPartitionBuffer buffer, byte[] data);
	}
}
