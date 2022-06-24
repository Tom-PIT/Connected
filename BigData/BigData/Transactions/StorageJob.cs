using System;
using System.Threading;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageJob : DispatcherJob<IQueueMessage>
	{
		public StorageJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		private Guid Block { get; set; }
		protected override void DoWork(IQueueMessage item)
		{
			Block = new Guid(item.Message);

			var block = Tenant.GetService<ITransactionService>().Select(Block);

			if (block is null)
			{
				Tenant.GetService<ILoggingService>().Dump($"StorageJob, {Block} block null.");
				Tenant.GetService<ITransactionService>().Complete(item.PopReceipt, new Guid(item.Message));
				
				return;
			}

			StoragePool.Enqueue(new StorageWorkerItem(block, item));
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Tenant.GetService<ILoggingService>().Dump($"StorageJob, {Block} block exception: {ex.Message}.");
			Tenant.LogError(nameof(StorageJob), ex.Message, LogCategories.BigData);
		}
	}
}