using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Persistence;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Partitions
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

			var partition = new Guid(item.Message);

			_timeout = new TimeoutTask(() =>
			{
				Instance.Tenant.GetService<IPartitionMaintenanceService>().Ping(Message.PopReceipt);
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
			var files = Instance.Tenant.GetService<IPartitionService>().QueryFiles(partition);

			foreach (var file in files)
				Instance.Tenant.GetService<IPersistenceService>().SynchronizeSchema(Instance.Tenant.GetService<INodeService>().Select(file.Node), file);

			Instance.Tenant.GetService<IPartitionMaintenanceService>().Complete(queue.PopReceipt, partition);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Tenant.LogError(nameof(MaintenanceJob), ex.Source, ex.Message);
			Instance.Tenant.GetService<IPartitionMaintenanceService>().Ping(item.PopReceipt);
		}
	}
}