using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionDispatcher : Dispatcher<IQueueMessage>
	{
		public SubscriptionDispatcher(string resourceGroup) : base(128)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new SubscriptionJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
