using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class QueueWorkerDispatcher : Dispatcher<IQueueMessage>
	{
		public QueueWorkerDispatcher(CancellationToken cancel) : base(cancel, 128)
		{
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationToken cancel)
		{
			return new QueueWorkerJob(this, cancel);
		}
	}
}
