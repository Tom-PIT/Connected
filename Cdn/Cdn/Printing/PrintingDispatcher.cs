using System.Threading;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingDispatcher : Dispatcher<IPrintQueueMessage>
	{
		public PrintingDispatcher(string resourceGroup) : base(2)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IPrintQueueMessage> CreateWorker(IDispatcher<IPrintQueueMessage> owner, CancellationToken cancel)
		{
			return new PrintJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
