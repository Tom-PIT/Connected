using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Worker.Services
{
	internal class WorkerService : HostedService
	{
		private Lazy<List<WorkerDispatcher>> _dispatchers = new Lazy<List<WorkerDispatcher>>();

		public WorkerService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var available = f.Available;

				if (available == 0)
					return;

				var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("WorkerManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", available},
					{ "resourceGroup", f.ResourceGroup }
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

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new WorkerDispatcher(i));

			return true;
		}
		private List<WorkerDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
