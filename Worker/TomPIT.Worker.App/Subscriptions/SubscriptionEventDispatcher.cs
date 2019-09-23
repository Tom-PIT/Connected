using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionEventDispatcher : Dispatcher<IQueueMessage>
	{
		public SubscriptionEventDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new SubscriptionEventJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
