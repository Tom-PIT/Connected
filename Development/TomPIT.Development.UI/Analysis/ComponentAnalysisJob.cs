using System;
using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.Diagostics;
using TomPIT.Distributed;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisJob : DispatcherJob<IComponentDevelopmentState>
	{
		public ComponentAnalysisJob(Dispatcher<IComponentDevelopmentState> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		private ComponentAnalysisDispatcher Dispatcher => Owner as ComponentAnalysisDispatcher;

		protected override void DoWork(IComponentDevelopmentState item)
		{
		}


		protected override void OnError(IComponentDevelopmentState item, Exception ex)
		{
			Dispatcher.Tenant.LogError(nameof(AutoFixJob), ex.Source, ex.Message);
		}
	}
}
