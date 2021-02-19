using System;
using System.Threading;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
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

			var block = MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Select(Block);

			if (block == null)
			{
				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"StorageJob, {Block} block null.");
				MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Complete(item.PopReceipt, new Guid(item.Message));
				return;
			}

			StoragePool.Enqueue(new StorageWorkerItem(block, item));
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"StorageJob, {Block} block exception: {ex.Message}.");
			MiddlewareDescriptor.Current.Tenant.LogError(nameof(StorageJob), ex.Message, LogCategories.BigData);
		}
	}
}