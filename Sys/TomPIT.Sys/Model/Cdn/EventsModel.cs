using System;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Model.Cdn
{
	internal class EventsModel : SynchronizedRepository<IEventDescriptor, Guid>
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

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Events.Select(id);

			if (r != null)
			{
				Set(id, r, TimeSpan.Zero);
				return;
			}

			Remove(id);
		}

		public Guid Insert(Guid microService, string name, string e, string callback)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms is null)
				throw new SysException(SR.ErrMicroServiceNotFound);

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

			descriptor.Id = Shell.GetService<IDatabaseService>().Proxy.Events.Insert(ms, name, descriptor.Identifier, DateTime.UtcNow, e, callback);
			DataModel.Queue.Enqueue(Queue, JsonConvert.SerializeObject(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			Set(descriptor.Identifier, descriptor, TimeSpan.Zero);

			return descriptor.Identifier;
		}

		public ImmutableList<IQueueMessage> Dequeue(int count)
		{
			return DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(5), QueueScope.System, Queue);
		}

		public IEventDescriptor Select(Guid id)
		{
			return Get(id);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e is null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Events.Delete(e);

			Remove(e.Identifier);
			DataModel.Queue.Complete(popReceipt);
		}

		private IEventDescriptor Resolve(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			var id = d.Required<Guid>("id");

			return Select(id);
		}
	}
}
