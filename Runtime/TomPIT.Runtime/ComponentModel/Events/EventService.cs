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

		public Guid Trigger(Guid microService, string name, IEventCallback callback)
		{
			return Trigger<object>(microService, name, callback, null);
		}

		public Guid Trigger<T>(Guid microService, string name, IEventCallback callback, T e)
		{
			var ev = Connection.GetService<IComponentService>().SelectComponent(microService, "Event", name);

			if (ev == null)
				throw new TomPITException(SR.ErrEventNotDefined);

			var u = Connection.CreateUrl("Event", "Trigger");
			var args = new JObject
			{
				{"microService", microService },
				{"name", name },
				{"callback", $"{callback.MicroService}/{callback.Api}/{callback.Operation}" }
			};

			if (e != null)
				args.Add("arguments", Types.Serialize(e));

			return Connection.Post<Guid>(u, args);
		}
	}
}
