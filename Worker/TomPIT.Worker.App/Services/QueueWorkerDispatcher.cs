using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class QueueWorkerDispatcher : Dispatcher<IQueueMessage>
	{
		public QueueWorkerDispatcher(CancellationTokenSource cancel) : base(cancel, 128)
		{
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new QueueWorkerJob(this, cancel);
		}
	}
}
