using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionEventDispatcher : Dispatcher<IQueueMessage>
	{
		public SubscriptionEventDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new SubscriptionEventJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
