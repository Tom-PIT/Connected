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

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationToken cancel)
		{
			return new SubscriptionEventJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
