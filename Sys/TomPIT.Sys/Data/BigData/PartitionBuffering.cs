using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data.BigData
{
	internal class PartitionBuffering : SynchronizedRepository<IPartitionBuffer, Guid>
	{
		private object _sync = new object();
		public PartitionBuffering(IMemoryCache container) : base(container, "bigdatabuffers")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Query();

			foreach (var i in ds)
				Set(i.Partition, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public List<IPartitionBuffer> Dequeue(int count, TimeSpan nextVisible)
		{
			return Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Dequeue(count, DateTime.UtcNow, DateTime.UtcNow.Add(nextVisible));
		}

		public void Enqueue(Guid partition, TimeSpan duration, byte[] data)
		{
			if (data == null || data.Length == 0)
				return;

			var p = Select(partition);

			if (p == null)
			{
				lock (_sync)
				{
					p = Select(partition);

					if (p == null)
					{
						Insert(partition, DateTime.UtcNow.Add(duration));
						p = Select(partition);
					}
				}
			}

			Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.InsertData(p, data);
		}
		private void Insert(Guid partition, DateTime nextVisible)
		{
			Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Insert(partition, nextVisible);
			Refresh(partition);
		}
		public void Update(Guid partition, TimeSpan nextVisible)
		{
			var p = Select(partition);

			if (p == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Update(p, DateTime.UtcNow.Add(nextVisible));
			Refresh(partition);
		}
		private IPartitionBuffer Select(Guid partition)
		{
			return Get(partition);
		}

		public List<IPartitionBufferData> QueryData(Guid partition)
		{
			var p = Select(partition);

			if (p == null)
				return null;

			return Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.QueryData(p);
		}

		public void Clear(Guid partition, TimeSpan nextVisible, long id)
		{
			var p = Select(partition);

			if (id > 0)
				Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Clear(p, id);

			Update(partition, nextVisible);
		}
	}
}
