using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Events
{
	internal class EventService : HostedService
	{
		private Lazy<List<EventDispatcher>> _dispatchers = new Lazy<List<EventDispatcher>>();

		public static EventService ServiceInstance { get; private set; } 

		public EventService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
			ServiceInstance = this;
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new EventDispatcher(i));

			return true;
		}
		protected override Task OnExecute(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				if (f.Available <= 0)
					return;

				if (cancel.IsCancellationRequested)
					return;

				if( MiddlewareDescriptor.Current.Tenant.Post<List<EventQueueMessage>>(MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Dequeue"), 
					new
					{
						count = f.Available,
						f.ResourceGroup
					}) is not List<EventQueueMessage> jobs)
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

		public List<EventDispatcher> Dispatchers { get { return _dispatchers.Value; } }

		public override void Dispose()
		{
			foreach (var dispatcher in Dispatchers)
				dispatcher.Dispose();

			Dispatchers.Clear();

			base.Dispose();
		}
	}
}
