using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Distributed;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisRunner : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<ComponentAnalysisDispatcher>> _dispatchers = new Lazy<List<ComponentAnalysisDispatcher>>();

		public ComponentAnalysisRunner()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(5000);
		}

		protected override bool Initialize()
		{
			var tenants = Shell.GetService<IConnectivityService>()?.QueryTenants();

			if (tenants != null)
			{
				foreach (var tenant in tenants)
					Dispatchers.Add(new ComponentAnalysisDispatcher(Shell.GetService<IConnectivityService>().SelectTenant(tenant.Url), _cancel));
			}

			return tenants != null;
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				//var jobs = f.Tenant.GetService<IDesignerService>().Dequeue(f.Available);

				//if (jobs == null)
				//	return;

				//foreach (var i in jobs)
				//	f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<ComponentAnalysisDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
