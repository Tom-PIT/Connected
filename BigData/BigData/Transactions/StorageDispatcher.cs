using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageDispatcher : Dispatcher<IQueueMessage>
	{
		public StorageDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new StorageJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
