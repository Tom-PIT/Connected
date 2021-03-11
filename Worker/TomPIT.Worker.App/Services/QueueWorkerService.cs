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
		private Lazy<List<QueueWorkerDispatcher>> _dispatchers = new Lazy<List<QueueWorkerDispatcher>>();

		public QueueWorkerService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			Dispatchers.Add(new QueueWorkerDispatcher());

			return true;
		}
		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available }
				};

				var jobs = MiddlewareDescriptor.Current.Tenant.Post<List<QueueMessage>>(url, e);

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

		private List<QueueWorkerDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
