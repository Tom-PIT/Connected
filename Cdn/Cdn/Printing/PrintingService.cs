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
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<PrintingDispatcher>> _dispatchers = new Lazy<List<PrintingDispatcher>>();

		protected override bool Initialize()
		{
			if (Instance.State == InstanceState.Initialining)
				return false;

			IntervalTimeout = TimeSpan.FromSeconds(5);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new PrintingDispatcher(i, _cancel));

			return true;
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Dequeue(f.Available);

				if (jobs == null)
					return;

				foreach (var job in jobs)
					f.Enqueue(job);
			});

			return Task.CompletedTask;
		}

		private List<PrintingDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
