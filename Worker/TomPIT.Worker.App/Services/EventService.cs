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
	internal class EventService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<EventDispatcher>> _dispatchers = new Lazy<List<EventDispatcher>>();

		public EventService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new EventDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Dequeue");

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
					f.Enqueue(i);
				}
			});

			return Task.CompletedTask;
		}

		private List<EventDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
