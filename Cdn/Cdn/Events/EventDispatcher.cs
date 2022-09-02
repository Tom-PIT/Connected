using System.Threading;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Events
{
	internal class EventDispatcher : Dispatcher<IEventQueueMessage>
	{
		public EventDispatcher(string resourceGroup) : base(256)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IEventQueueMessage> CreateWorker(IDispatcher<IEventQueueMessage> owner, CancellationToken cancel)
		{
			return new EventJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
