using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageDispatcher : Dispatcher<IQueueMessage>
	{
		public StorageDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 16)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationToken cancel)
		{
			return new StorageJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
