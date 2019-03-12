using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.Worker.Services
{
	internal class SubscriptionEventService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<SubscriptionEventDispatcher>> _dispatchers = new Lazy<List<SubscriptionEventDispatcher>>();

		public SubscriptionEventService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new SubscriptionEventDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Connection.CreateUrl("SubscriptionManagement", "DequeueEvents");

				var e = new JObject
				{
					{ "count", f.Available },
					{ "resourceGroup", f.ResourceGroup }
				};

				var jobs = Instance.Connection.Post<List<QueueMessage>>(url, e);

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
