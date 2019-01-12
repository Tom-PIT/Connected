using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.Worker.Services
{
	internal class WorkerService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<WorkerDispatcher>> _dispatchers = new Lazy<List<WorkerDispatcher>>();

		public WorkerService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Instance.ResourceGroups)
				Dispatchers.Add(new WorkerDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Connection.CreateUrl("WorkerManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
					{ "resourceGroup", f.ResourceGroup }
				};

				var jobs = Instance.Connection.Post<List<QueueMessage>>(url, e);

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
