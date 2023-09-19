using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Middleware;

namespace TomPIT.BigData.Transactions
{
	internal class StorageService : HostedService
	{
		private Lazy<List<StorageDispatcher>> _dispatchers = new Lazy<List<StorageDispatcher>>();

		public StorageService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			StoragePool.Cancel = cancel;

			foreach (var i in MiddlewareDescriptor.Current.Tenant.GetService<IResourceGroupService>().QuerySupported())
				Dispatchers.Add(new StorageDispatcher(i.Name));

			return true;
		}
		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var available = f.Available;

				if (available == 0)
					return;

				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Dequeue(available);

				if (cancel.IsCancellationRequested)
					return;

				if (jobs == null)
					return;

				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"StorageService, {jobs.Count} jobs dequeued.");

				foreach (var i in jobs)
				{
					if (cancel.IsCancellationRequested)
						return;

					f.Enqueue(i);
				}
			});

			return Task.CompletedTask;
		}

		private List<StorageDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
