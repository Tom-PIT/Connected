using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Cdn.Events
{
	internal class EventDispatcher : Dispatcher<IQueueMessage>
	{
		public EventDispatcher(string resourceGroup) : base(256)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new EventJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
