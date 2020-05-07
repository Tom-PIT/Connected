using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.BigData.Partitions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public StorageJob(Dispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		private IQueueMessage Message { get; set; }

		protected override void DoWork(IQueueMessage item)
		{
			Message = item;

			var block = new Guid(item.Message);

			_timeout = new TimeoutTask(() =>
			{
				MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Ping(Message.PopReceipt);
				return Task.CompletedTask;
			}, TimeSpan.FromSeconds(45), Cancel);

			_timeout.Start();

			try
			{
				Invoke(item, block);
			}
			finally
			{
				_timeout.Stop();
			}
		}

		private void Invoke(IQueueMessage queue, Guid blockId)
		{
			var block = MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Select(blockId);

			if (block == null)
				return;

			ValidateSchema(block);

			var updater = new Updater(block);

			updater.Execute();

			if (updater.LockedItems != null && updater.LockedItems.Count > 0)
			{
				if (updater.UpdateRowCount == 0)
				{
					MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Ping(queue.PopReceipt);
					return;
				}
				else
				{
					var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(block.Partition) as IPartitionConfiguration;

					MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Prepare(config, updater.LockedItems);
				}
			}

			MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Complete(queue.PopReceipt, blockId);
		}

		private void ValidateSchema(ITransactionBlock block)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().ValidateSchema(block.Partition);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(StorageJob));
			MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Ping(item.PopReceipt);
		}
	}
}