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
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<WorkerDispatcher>> _dispatchers = new Lazy<List<WorkerDispatcher>>();

		public WorkerService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new WorkerDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Tenant.CreateUrl("WorkerManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
					{ "resourceGroup", f.ResourceGroup }
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

		private List<WorkerDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
