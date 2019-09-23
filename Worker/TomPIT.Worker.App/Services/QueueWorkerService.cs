using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Worker.Services
{
	internal class QueueWorkerService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<QueueWorkerDispatcher>> _dispatchers = new Lazy<List<QueueWorkerDispatcher>>();

		public QueueWorkerService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			Dispatchers.Add(new QueueWorkerDispatcher(_cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Tenant.CreateUrl("QueueManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available }
				};

				var jobs = Instance.Tenant.Post<List<QueueMessage>>(url, e);

				if (jobs == null)
					return;

				foreach (var i in jobs)
				{
					f.Enqueue(i);
				}
			});

			return Task.CompletedTask;
		}

		private List<QueueWorkerDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
