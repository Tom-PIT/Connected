using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.BigData.Partitions
{
	internal class MaintenanceService : HostedService
	{
		private Lazy<List<MaintenanceDispatcher>> _dispatchers = new Lazy<List<MaintenanceDispatcher>>();

		public MaintenanceService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(4900);
		}

		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new MaintenanceDispatcher(i, cancel));

			return true;
		}

		protected override Task Process(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<IPartitionMaintenanceService>().Dequeue(f.Available);

				if (cancel.IsCancellationRequested)
					return;

				if (jobs == null)
					return;

				foreach (var i in jobs)
				{
					if (cancel.IsCancellationRequested)
						return;

					f.Enqueue(i);
				}
			});

			return Task.CompletedTask;
		}

		private List<MaintenanceDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
