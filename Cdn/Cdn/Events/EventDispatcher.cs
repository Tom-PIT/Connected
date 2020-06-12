using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Cdn.Events
{
	internal class EventDispatcher : Dispatcher<IQueueMessage>
	{
		public EventDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationToken cancel)
		{
			return new EventJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
