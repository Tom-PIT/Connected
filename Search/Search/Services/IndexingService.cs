using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Middleware;

namespace TomPIT.Search.Services
{
	internal class IndexingService : HostedService
	{
		private Lazy<List<IndexingDispatcher>> _dispatchers = new Lazy<List<IndexingDispatcher>>();

		public IndexingService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Tenant.GetService<IResourceGroupService>().QuerySupported())
				Dispatchers.Add(new IndexingDispatcher(i.Name));
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			return Instance.State == InstanceState.Running;
		}

		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
					{ "resourceGroup", f.ResourceGroup }
				};

				var jobs = MiddlewareDescriptor.Current.Tenant.Post<List<QueueMessage>>(url, e);

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

		private List<IndexingDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
