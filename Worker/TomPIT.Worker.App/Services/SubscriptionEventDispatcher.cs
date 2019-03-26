using System.Threading;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
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
