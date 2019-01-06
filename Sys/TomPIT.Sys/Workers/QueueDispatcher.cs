using System;
using System.Threading;
using TomPIT.Services;

namespace TomPIT.Sys.Workers
{
	internal class QueueDispatcher : Dispatcher<Guid>
	{
		public QueueDispatcher(CancellationTokenSource cancel) : base(cancel, 32)
		{
		}

		protected override DispatcherJob<Guid> CreateWorker(CancellationTokenSource cancel)
		{
			return new QueueJob(this, cancel);
		}
	}
}
