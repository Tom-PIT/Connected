using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TomPIT.Diagnostics;

namespace TomPIT.BigData.Transactions
{
	internal static class StoragePool
	{
		private static Lazy<ConcurrentDictionary<Guid, ConcurrentQueue<StorageWorkerItem>>> _items = new Lazy<ConcurrentDictionary<Guid, ConcurrentQueue<StorageWorkerItem>>>();
		private static Lazy<ConcurrentDictionary<Guid, StorageWorker>> _workers = new Lazy<ConcurrentDictionary<Guid, StorageWorker>>();
		public static void Enqueue(StorageWorkerItem item)
		{
			var queue = EnsureQueue(item.Block.Partition);

			if (queue == null)
				return;

			queue.Enqueue(item);

			EnsureWorker(item);
		}

		public static CancellationToken Cancel { get; set; }

		private static ConcurrentDictionary<Guid, ConcurrentQueue<StorageWorkerItem>> Items => _items.Value;
		private static ConcurrentDictionary<Guid, StorageWorker> Workers => _workers.Value;

		public static bool Dequeue(Guid partition, out StorageWorkerItem item)
		{
			item = null;

			if (!Items.TryGetValue(partition, out ConcurrentQueue<StorageWorkerItem> queue))
				return false;

			if (!queue.TryDequeue(out item))
				return false;

			return true;
		}

		private static ConcurrentQueue<StorageWorkerItem> EnsureQueue(Guid partition)
		{
			if (Items.TryGetValue(partition, out ConcurrentQueue<StorageWorkerItem> queue))
				return queue;

			queue = new ConcurrentQueue<StorageWorkerItem>();

			if (Items.TryAdd(partition, queue))
				return queue;

			return Items.GetValueOrDefault(partition);
		}

		private static void EnsureWorker(StorageWorkerItem item)
		{
			if (!Workers.ContainsKey(item.Block.Partition))
			{
				var worker = new StorageWorker(item.Block.Partition, Cancel);

				worker.Completed += OnWorkerCompleted;
				Workers.TryAdd(item.Block.Partition, worker);

				worker.Run();
			}
		}

		private static void OnWorkerCompleted(object sender, EventArgs e)
		{
			if (sender is not StorageWorker worker)
				return;

			try
			{
				Tenant.GetService<ILoggingService>().Dump($"StoragePool, {worker.Partition} partition worker completed.");

				if (Workers.ContainsKey(worker.Partition))
					Workers.TryRemove(worker.Partition, out StorageWorker _);

				worker.Dispose();
				worker = null;
			}
			catch { }
		}
	}
}
