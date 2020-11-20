using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingService : HostedService
	{
		private Lazy<List<PrintingDispatcher>> _dispatchers = new Lazy<List<PrintingDispatcher>>();

		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			IntervalTimeout = TimeSpan.FromSeconds(5);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new PrintingDispatcher(i, cancel));

			return true;
		}

		protected override Task Process(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Dequeue(f.Available);

				if (cancel.IsCancellationRequested)
					return;

				if (jobs == null)
					return;

				foreach (var job in jobs)
				{
					if (cancel.IsCancellationRequested)
						return;

					f.Enqueue(job);
				}
			});

			return Task.CompletedTask;
		}

		private List<PrintingDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
