using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingDispatcher : Dispatcher<IQueueMessage>
	{
		public PrintingDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 16)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new PrintJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
