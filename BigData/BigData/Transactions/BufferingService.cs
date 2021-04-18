using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LZ4;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
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
			Tenant.Post(CreateUrl("ClearBufferData"), new
			{
				Partition = partition,
				NextVisible = nextVisible,
				Id = id
			});
		}

		public List<IPartitionBuffer> Dequeue(int count)
		{
			return Tenant.Post<List<PartitionBuffer>>(CreateUrl("DequeueBuffers"), new
			{
				Count = count,
				TimeSpan = TimeSpan.FromSeconds(60)
			}).ToList<IPartitionBuffer>();
		}

		public void Enqueue(Guid partition, TimeSpan duration, JArray items)
		{
			if (items == null || items.Count == 0)
				return;

			var raw = Encoding.UTF8.GetBytes(Serializer.Serialize(items));

			Tenant.Post(CreateUrl("EnqueueBuffer"), new
			{
				Partition = partition,
				Duration = duration,
				Data = LZ4Codec.Wrap(raw)
			});
		}

		public List<IPartitionBufferData> QueryData(Guid partition)
		{
			return Tenant.Post<List<PartitionBufferData>>(CreateUrl("QueryBufferData"), new
			{
				Partition = partition,
			}).ToList<IPartitionBufferData>();
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl("BigDataManagement", action);
		}
	}
}
