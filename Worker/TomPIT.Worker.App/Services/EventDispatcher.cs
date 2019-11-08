using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class EventDispatcher : Dispatcher<IQueueMessage>
	{
		public EventDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new EventJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
