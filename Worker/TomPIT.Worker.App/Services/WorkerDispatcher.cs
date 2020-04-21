using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class WorkerDispatcher : Dispatcher<IQueueMessage>
	{
		public WorkerDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationToken cancel)
		{
			return new WorkerJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
