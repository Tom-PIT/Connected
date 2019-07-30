using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	internal class MaintenanceJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public MaintenanceJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		private IQueueMessage Message { get; set; }

		protected override void DoWork(IQueueMessage item)
		{
			Message = item;

			var partition = item.Message.AsGuid();

			_timeout = new TimeoutTask(() =>
			{
				Instance.GetService<IPartitionMaintenanceService>().Ping(Message.PopReceipt);
				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(15));

			_timeout.Start();

			try
			{
				Invoke(item, partition);
			}
			finally
			{
				_timeout.Stop();
			}
		}

		private void Invoke(IQueueMessage queue, Guid partition)
		{
			var files = Instance.GetService<IPartitionService>().QueryFiles(partition);

			foreach (var file in files)
				Instance.GetService<IPersistenceService>().SynchronizeSchema(Instance.GetService<INodeService>().Select(file.Node), file);

			Instance.GetService<IPartitionMaintenanceService>().Complete(queue.PopReceipt, partition);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(MaintenanceJob), ex.Source, ex.Message);
			Instance.Connection.GetService<IPartitionMaintenanceService>().Ping(item.PopReceipt);
		}
	}
}