using System;
using System.Threading;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageJob : DispatcherJob<IQueueMessage>
	{
		public StorageJob(Dispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var block = MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Select(new Guid(item.Message));

			if (block == null)
			{
				MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Complete(item.PopReceipt, new Guid(item.Message));
				return;
			}

			StoragePool.Enqueue(new StorageWorkerItem(block, item));
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{

		}
	}
}