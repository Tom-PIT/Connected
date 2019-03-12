using System.Threading;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class SubscriptionDispatcher : Dispatcher<IClientQueueMessage>
	{
		public SubscriptionDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IClientQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new SubscriptionJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
