using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Ide.Designers;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisRunner : HostedService
	{
		private Lazy<List<ComponentAnalysisDispatcher>> _dispatchers = new Lazy<List<ComponentAnalysisDispatcher>>();

		public ComponentAnalysisRunner()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(5000);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			var tenants = Shell.GetService<IConnectivityService>()?.QueryTenants();

			if (tenants != null)
			{
				foreach (var tenant in tenants)
					Dispatchers.Add(new ComponentAnalysisDispatcher(Shell.GetService<IConnectivityService>().SelectTenant(tenant.Url)));
			}

			return tenants != null;
		}

		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				if (f.Tenant.GetService<IDesignerService>().DequeueDevelopmentStates(f.Available) is not List<IComponentDevelopmentState> jobs)
					return;

				foreach (var i in jobs)
					f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<ComponentAnalysisDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
