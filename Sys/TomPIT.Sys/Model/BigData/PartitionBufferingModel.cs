using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Caching;

namespace TomPIT.Sys.Model.BigData
{
	internal class PartitionBufferingModel : IdentityRepository<PartitionBuffer, Guid>
	{
		private readonly object _sync = new object();
		public PartitionBufferingModel(IMemoryCache container) : base(container, "bigdatabuffers")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Query();

			foreach (var i in ds)
			{
				if (i.Id > Identity)
					Seed(i.Id);

				Set(i.Partition, new PartitionBuffer(i), TimeSpan.Zero);
			}
		}

		public List<PartitionBuffer> Dequeue(int count, TimeSpan nextVisible)
		{
			var result = new List<PartitionBuffer>();

			foreach (var item in All().OrderBy(f=>f.NextVisible))
			{
				if (item.NextVisible > DateTime.UtcNow)
					continue;

				item.NextVisible = DateTime.UtcNow.Add(nextVisible);

				result.Add(item);

				Dirty();

				if (result.Count >= count)
					break;
			}

			return result;
		}

		public void Enqueue(Guid partition, TimeSpan duration, byte[] data)
		{
			if (data == null || data.Length == 0)
				return;

			if (Select(partition) is not IPartitionBuffer buffer)
			{
				lock (_sync)
				{
					if (Select(partition) is not IPartitionBuffer innerBuffer)
					{
						Insert(partition, DateTime.UtcNow.Add(duration));
						buffer = Select(partition);
					}
					else
						buffer = innerBuffer;
				}
			}

			Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.InsertData(buffer, data);
		}
		private void Insert(Guid partition, DateTime nextVisible)
		{
			var buffer = new PartitionBuffer
			{
				Id = Convert.ToInt32(Increment()),
				NextVisible = nextVisible,
				Partition = partition
			};

			Set(buffer.Partition, buffer, TimeSpan.Zero);

			Dirty();
			/*
			 * We need to flush it immediatelly because 
			 * data is waiting for the inserted parent record
			 */
			OnFlushing().Wait();
		}
		public void Update(Guid partition, TimeSpan nextVisible)
		{
			if (Select(partition) is not PartitionBuffer buffer)
				return;

			buffer.NextVisible = DateTime.UtcNow.Add(nextVisible);

			Dirty();
		}
		private IPartitionBuffer Select(Guid partition)
		{
			return Get(partition);
		}

		public List<IPartitionBufferData> QueryData(Guid partition)
		{
			if (Select(partition) is not IPartitionBuffer buffer)
				return null;

			return Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.QueryData(buffer);
		}

		public void Clear(Guid partition, TimeSpan nextVisible, long id)
		{
			var p = Select(partition);

			if (id > 0)
				Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Clear(p, id);

			Update(partition, nextVisible);
		}

		protected override async Task OnFlushing()
		{
			Shell.GetService<IDatabaseService>().Proxy.BigData.Buffer.Update(All().ToList<IPartitionBuffer>());

			await Task.CompletedTask;
		}
	}
}
