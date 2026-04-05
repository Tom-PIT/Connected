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
			var partition = item.Block.Partition;

			if (Workers.ContainsKey(partition))
				return;

			var worker = new StorageWorker(partition, Cancel);
			worker.Completed += OnWorkerCompleted;

			if (Workers.TryAdd(partition, worker))
			{
				worker.Run();
			}
			else
			{
				worker.Completed -= OnWorkerCompleted;
				worker.Dispose();
			}
		}

		private static void OnWorkerCompleted(object sender, EventArgs e)
		{
			if (sender is not StorageWorker worker)
				return;

			try
			{
				Tenant.GetService<ILoggingService>().Dump($"StoragePool, {worker.Partition} partition worker completed.");

				Workers.TryRemove(worker.Partition, out _);

				worker.Dispose();
				worker = null;
			}
			catch { }
		}
	}
}
