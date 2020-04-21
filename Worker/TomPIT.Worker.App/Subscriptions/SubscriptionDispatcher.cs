using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionDispatcher : Dispatcher<IQueueMessage>
	{
		public SubscriptionDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationToken cancel)
		{
			return new SubscriptionJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
