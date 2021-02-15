using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Partitions
{
	internal class MaintenanceDispatcher : Dispatcher<IQueueMessage>
	{
		public MaintenanceDispatcher(string resourceGroup) : base(4)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new MaintenanceJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
