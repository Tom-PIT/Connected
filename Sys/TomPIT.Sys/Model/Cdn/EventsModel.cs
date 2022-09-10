using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Caching;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Model.Cdn
{
	internal class EventsModel : PersistentRepository<IEventDescriptor, Guid>
	{
		private const string Queue = "event";

		public EventsModel(IMemoryCache container) : base(container, "events")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Events.Query();

			foreach (var j in ds)
				Set(j.Identifier, j, TimeSpan.Zero);
		}

		public Guid Insert(Guid microService, string name, string e, string callback)
		{
			var descriptor = new EventDescriptor
			{
				Arguments = e,
				Callback = callback,
				Created = DateTime.UtcNow,
				Identifier = Guid.NewGuid(),
				MicroService = microService,
				Name = name
			};

			var message = new JObject
			{
				{ "id", descriptor.Identifier}
			};

			Set(descriptor.Identifier, descriptor, TimeSpan.Zero);

			DataModel.Queue.Enqueue(Queue, JsonConvert.SerializeObject(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			Dirty();

			return descriptor.Identifier;
		}

		public ImmutableList<IEventQueueMessage> Dequeue(int count)
		{
#if DEBUG
			var sw = new Stopwatch();

			sw.Start();
#endif

			var messages = DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(5), QueueScope.System, Queue);

			if (!messages.Any())
				return ImmutableList<IEventQueueMessage>.Empty;

			var result = new List<IEventQueueMessage>();

			foreach (var message in messages)
			{
				var m = JsonConvert.DeserializeObject(message.Message) as JObject;

				if (m is null || m.Required<Guid>("id") == Guid.Empty)
					continue;

				var id = m.Required<Guid>("id");

				if (id == Guid.Empty)
					continue;

				if (Select(id) is IEventDescriptor ev)
					result.Add(new EventQueueMessage(message, ev));
				else
					DataModel.Queue.Complete(message.PopReceipt);
			}

#if DEBUG
			sw.Stop();

			if (sw.ElapsedMilliseconds > 1)
				Debug.WriteLine($"Dequeue {sw.ElapsedMilliseconds:n0} ms.", "Events");
#endif

			return result.ToImmutableList();
		}

		public IEventDescriptor Select(Guid id)
		{
			return Get(id);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			if (DataModel.Queue.Select(popReceipt) is IQueueMessage message)
			{
				if (Resolve(message) is IEventDescriptor e)
				{
					Remove(e.Identifier);
					Dirty();
				}
			}

			DataModel.Queue.Complete(popReceipt);
		}

		private IEventDescriptor Resolve(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			return Select(d.Required<Guid>("id"));
		}

		protected override async Task OnFlushing()
		{
			var events = All();

#if DEBUG
			var sw = new Stopwatch();

			sw.Start();
#endif
			Shell.GetService<IDatabaseService>().Proxy.Events.Update(events);
			await Task.CompletedTask;
#if DEBUG
			sw.Stop();

			Debug.WriteLine($"Events update {sw.ElapsedMilliseconds:n0} ms. {events.Count} items.", "Events");
#endif
		}
	}
}
