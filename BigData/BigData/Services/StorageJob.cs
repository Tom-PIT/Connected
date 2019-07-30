using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
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

			var block = item.Message.AsGuid();

			_timeout = new TimeoutTask(() =>
			{
				Instance.GetService<ITransactionService>().Ping(Message.PopReceipt);
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
			var block = Instance.Connection.GetService<ITransactionService>().Select(blockId);

			if (block == null)
				return;

			ValidateSchema(block);

			var updater = new Updater(block);

			updater.Execute();

			if (updater.LockedItems != null && updater.LockedItems.Count > 0)
			{
				if (updater.UpdateRowCount == 0)
				{
					Instance.Connection.GetService<ITransactionService>().Ping(queue.PopReceipt);
					return;
				}
				else
				{
					var config = Instance.Connection.GetService<IComponentService>().SelectConfiguration(block.Partition) as IPartitionConfiguration;

					Instance.GetService<ITransactionService>().Prepare(config, updater.LockedItems);
				}
			}

			Instance.GetService<ITransactionService>().Complete(queue.PopReceipt, blockId);
		}

		private void ValidateSchema(ITransactionBlock block)
		{
			Instance.GetService<IPartitionService>().ValidateSchema(block.Partition);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(StorageJob), ex.Source, ex.Message);
			Instance.Connection.GetService<ITransactionService>().Ping(item.PopReceipt);
		}
	}
}