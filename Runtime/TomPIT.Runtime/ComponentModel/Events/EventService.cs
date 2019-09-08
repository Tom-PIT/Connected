using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel.Events
{
	internal class EventService : IEventService
	{
		public EventService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; set; }

		public Guid Trigger(IDistributedEvent ev, IEventCallback callback)
		{
			return Trigger<object>(ev, callback, null);
		}

		public Guid Trigger<T>(IDistributedEvent ev, IEventCallback callback, T e)
		{
			var u = Connection.CreateUrl("Event", "Trigger");
			var args = new JObject
			{
				{"microService", ev.MicroService(Connection) },
				{"name", ev.ComponentName(Connection) }
			};

			if (callback != null)
				args.Add(new JObject { "callback", $"{callback.MicroService}/{callback.Api}/{callback.Operation}" });

			if (e != null)
				args.Add("arguments", Types.Serialize(e));

			return Connection.Post<Guid>(u, args);
		}
	}
}
