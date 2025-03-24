using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Search.Services
{
	internal class IndexingDispatcher : Dispatcher<IQueueMessage>
	{
		public IndexingDispatcher(string resourceGroup) : base(1)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new IndexingJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
