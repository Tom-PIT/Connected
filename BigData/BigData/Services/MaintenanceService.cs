using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.BigData.Services
{
	internal class MaintenanceService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<MaintenanceDispatcher>> _dispatchers = new Lazy<List<MaintenanceDispatcher>>();

		public MaintenanceService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(4900);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new MaintenanceDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = Instance.Connection.GetService<IPartitionMaintenanceService>().Dequeue(f.Available);

				if (jobs == null)
					return;

				foreach (var i in jobs)
					f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<MaintenanceDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
