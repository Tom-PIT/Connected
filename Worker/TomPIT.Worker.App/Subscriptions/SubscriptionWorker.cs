using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionWorker : HostedService
	{
		private Lazy<List<SubscriptionDispatcher>> _dispatchers = new Lazy<List<SubscriptionDispatcher>>();

		public SubscriptionWorker()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new SubscriptionDispatcher(i, cancel));

			return true;
		}
		protected override Task Process(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().DequeueSubscriptions(f.ResourceGroup, f.Available);

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

		private List<SubscriptionDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
