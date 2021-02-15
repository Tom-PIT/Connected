using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Events
{
	internal class EventService : HostedService
	{
		private Lazy<List<EventDispatcher>> _dispatchers = new Lazy<List<EventDispatcher>>();

		public EventService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new EventDispatcher(i, cancel));

			return true;
		}
		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				if (f.Available <= 0)
					return;

				var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
					{ "resourceGroup", f.ResourceGroup }
				};

				if (cancel.IsCancellationRequested)
					return;

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

		private List<EventDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
