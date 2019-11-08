using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.BigData.Transactions
{
	internal class StorageService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<StorageDispatcher>> _dispatchers = new Lazy<List<StorageDispatcher>>();

		public StorageService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new StorageDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Dequeue(f.Available);

				if (jobs == null)
					return;

				foreach (var i in jobs)
					f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<StorageDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
