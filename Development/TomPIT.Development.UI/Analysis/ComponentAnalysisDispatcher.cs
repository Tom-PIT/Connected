using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Distributed;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisDispatcher : Dispatcher<IComponentDevelopmentState>
	{
		public ComponentAnalysisDispatcher(ITenant tenant) : base(128)
		{
			Tenant = tenant;
		}

		public override DispatcherJob<IComponentDevelopmentState> CreateWorker(IDispatcher<IComponentDevelopmentState> owner, CancellationToken cancel)
		{
			return new ComponentAnalysisJob(owner, cancel);
		}

		public ITenant Tenant { get; }
	}
}
