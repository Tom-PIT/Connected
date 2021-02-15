using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageDispatcher : Dispatcher<IQueueMessage>
	{
		public StorageDispatcher(string resourceGroup) : base(32)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new StorageJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
