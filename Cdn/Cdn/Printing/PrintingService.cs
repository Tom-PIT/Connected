using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingService : HostedService
	{
		private Lazy<List<PrintingDispatcher>> _dispatchers = new Lazy<List<PrintingDispatcher>>();

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			IntervalTimeout = TimeSpan.FromSeconds(5);

			foreach (var i in Tenant.GetService<IResourceGroupService>().QuerySupported())
				Dispatchers.Add(new PrintingDispatcher(i.Name));

			return true;
		}

		protected override Task OnExecute(CancellationToken cancel)
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

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
