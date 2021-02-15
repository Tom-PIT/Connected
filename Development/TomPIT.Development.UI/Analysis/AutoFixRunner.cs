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
		private readonly Lazy<List<AutoFixDispatcher>> _dispatchers = new Lazy<List<AutoFixDispatcher>>();

		public AutoFixRunner()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(5000);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			var tenants = Shell.GetService<IConnectivityService>()?.QueryTenants();

			if (tenants != null)
			{
				foreach (var tenant in tenants)
					Dispatchers.Add(new AutoFixDispatcher(Shell.GetService<IConnectivityService>().SelectTenant(tenant.Url)));
			}

			return tenants != null;
		}

		protected override Task OnExecute(CancellationToken cancel)
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

		public override void Dispose()
		{
			foreach(var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
