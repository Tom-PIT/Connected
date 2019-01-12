using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.Worker.Services
{
	internal class EventService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<EventDispatcher>> _dispatchers = new Lazy<List<EventDispatcher>>();

		public EventService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);

			foreach (var i in Instance.ResourceGroups)
				Dispatchers.Add(new EventDispatcher(i, _cancel));
		}

		protected override Task Process()
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = Instance.Connection.CreateUrl("EventManagement", "Dequeue");

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

		private List<EventDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
