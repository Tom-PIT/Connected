using System.Threading;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Search.Services
{
	internal class IndexingDispatcher : Dispatcher<IQueueMessage>
	{
		public IndexingDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new IndexingJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
