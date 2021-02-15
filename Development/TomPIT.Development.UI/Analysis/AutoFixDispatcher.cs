using System.Threading;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Development.Analysis
{
	internal class AutoFixDispatcher : Dispatcher<IQueueMessage>
	{
		public AutoFixDispatcher(ITenant tenant) : base(128)
		{
			Tenant = tenant;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new AutoFixJob(owner, cancel);
		}

		public ITenant Tenant { get; }
	}
}
