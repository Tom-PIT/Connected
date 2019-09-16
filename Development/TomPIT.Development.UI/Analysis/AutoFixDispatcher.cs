using System.Threading;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Development.Analysis
{
	internal class AutoFixDispatcher : Dispatcher<IQueueMessage>
	{
		public AutoFixDispatcher(ITenant tenant, CancellationTokenSource cancel) : base(cancel, 128)
		{
			Tenant = tenant;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new AutoFixJob(this, cancel);
		}

		public ITenant Tenant { get; }
	}
}
