using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Distributed;

namespace TomPIT.Development.Analysis
{
	internal class AutoFixRunner : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<AutoFixDispatcher>> _dispatchers = new Lazy<List<AutoFixDispatcher>>();

		public AutoFixRunner()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(5000);

			var tenants = Shell.GetService<IConnectivityService>().QueryTenants();

			foreach (var tenant in tenants)
				Dispatchers.Add(new AutoFixDispatcher(Shell.GetService<IConnectivityService>().SelectTenant(tenant.Url), _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = f.Tenant.GetService<IAutoFixService>().Dequeue(f.Available);

				if (jobs == null)
					return;

				foreach (var i in jobs)
					f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<AutoFixDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
