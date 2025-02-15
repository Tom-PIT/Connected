﻿using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class QueueWorkerDispatcher : Dispatcher<IQueueMessage>
	{
		public QueueWorkerDispatcher() : base(4)
		{
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new QueueWorkerJob(owner, cancel);
		}
	}
}
