using System.Threading;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionDispatcher : Dispatcher<IQueueMessage>
	{
		public SubscriptionDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new SubscriptionJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
