using System;
using System.Threading;
using TomPIT.Services;

namespace TomPIT.Sys.Workers
{
	internal class QueueJob : DispatcherJob<Guid>
	{
		public QueueJob(Dispatcher<Guid> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(Guid item)
		{

		}
	}
}
