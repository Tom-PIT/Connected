﻿using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Distributed;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisDispatcher : Dispatcher<IComponentDevelopmentState>
	{
		public ComponentAnalysisDispatcher(ITenant tenant, CancellationTokenSource cancel) : base(cancel, 128)
		{
			Tenant = tenant;
		}

		protected override DispatcherJob<IComponentDevelopmentState> CreateWorker(CancellationTokenSource cancel)
		{
			return new ComponentAnalysisJob(this, cancel);
		}

		public ITenant Tenant { get; }
	}
}