using LZ4;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Connectivity;
using TomPIT.Serialization;

namespace TomPIT.BigData.Transactions
{
	internal class BufferingService : TenantObject, IBufferingService
	{
		public BufferingService(ITenant tenant) : base(tenant)
		{
		}

		public void ClearData(Guid partition, TimeSpan nextVisible, long id)
		{
			Instance.SysProxy.Management.BigData.ClearBufferData(partition, nextVisible, id);
		}

		public List<IPartitionBuffer> Dequeue(int count)
		{
			return Instance.SysProxy.Management.BigData.DequeueBuffers(count, TimeSpan.FromSeconds(60)).ToList();
		}

		public void Enqueue(Guid partition, TimeSpan duration, JArray items)
		{
			if (items == null || items.Count == 0)
				return;

			var raw = Encoding.UTF8.GetBytes(Serializer.Serialize(items));

			Instance.SysProxy.Management.BigData.EnqueueBuffer(partition, duration, LZ4Codec.Wrap(raw));
		}

		public List<IPartitionBufferData> QueryData(Guid partition)
		{
			return Instance.SysProxy.Management.BigData.QueryBufferData(partition).ToList();
		}
	}
}
