using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionEventWorker : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<SubscriptionEventDispatcher>> _dispatchers = new Lazy<List<SubscriptionEventDispatcher>>();

		public SubscriptionEventWorker()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new SubscriptionEventDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = Instance.Connection.GetService<ISubscriptionWorkerService>().DequeueSubscriptionEvents(f.ResourceGroup, f.Available);

				if (jobs == null)
					return;

				foreach (var i in jobs)
					f.Enqueue(i);
			});

			return Task.CompletedTask;
		}

		private List<SubscriptionEventDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
