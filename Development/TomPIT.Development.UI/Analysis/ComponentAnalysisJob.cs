using System;
using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;
using TomPIT.Distributed;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisJob : DispatcherJob<IComponentDevelopmentState>
	{
		public ComponentAnalysisJob(IDispatcher<IComponentDevelopmentState> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		private ComponentAnalysisDispatcher Dispatcher => Owner as ComponentAnalysisDispatcher;

		protected override void DoWork(IComponentDevelopmentState item)
		{
		}


		protected override void OnError(IComponentDevelopmentState item, Exception ex)
		{
			Dispatcher.Tenant.LogError(ex.Source, ex.Message, nameof(AutoFixJob));
		}
	}
}
