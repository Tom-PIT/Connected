using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingDispatcher : Dispatcher<IQueueMessage>
	{
		public PrintingDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 16)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new PrintJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
