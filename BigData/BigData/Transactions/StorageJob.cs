using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.BigData.Partitions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public StorageJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		private IQueueMessage Message { get; set; }

		protected override void DoWork(IQueueMessage item)
		{
			Message = item;

			var block = new Guid(item.Message);

			_timeout = new TimeoutTask(() =>
			{
				Instance.Tenant.GetService<ITransactionService>().Ping(Message.PopReceipt);
				return Task.CompletedTask;
			}, TimeSpan.FromSeconds(45));

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
			var block = Instance.Tenant.GetService<ITransactionService>().Select(blockId);

			if (block == null)
				return;

			ValidateSchema(block);

			var updater = new Updater(block);

			updater.Execute();

			if (updater.LockedItems != null && updater.LockedItems.Count > 0)
			{
				if (updater.UpdateRowCount == 0)
				{
					Instance.Tenant.GetService<ITransactionService>().Ping(queue.PopReceipt);
					return;
				}
				else
				{
					var config = Instance.Tenant.GetService<IComponentService>().SelectConfiguration(block.Partition) as IPartitionConfiguration;

					Instance.Tenant.GetService<ITransactionService>().Prepare(config, updater.LockedItems);
				}
			}

			Instance.Tenant.GetService<ITransactionService>().Complete(queue.PopReceipt, blockId);
		}

		private void ValidateSchema(ITransactionBlock block)
		{
			Instance.Tenant.GetService<IPartitionService>().ValidateSchema(block.Partition);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Tenant.LogError(nameof(StorageJob), ex.Source, ex.Message);
			Instance.Tenant.GetService<ITransactionService>().Ping(item.PopReceipt);
		}
	}
}