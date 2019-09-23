using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Partitions
{
	internal class MaintenanceDispatcher : Dispatcher<IQueueMessage>
	{
		public MaintenanceDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 4)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new MaintenanceJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
