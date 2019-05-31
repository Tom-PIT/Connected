using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;
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
