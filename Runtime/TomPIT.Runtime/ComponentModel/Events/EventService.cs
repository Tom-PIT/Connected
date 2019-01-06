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

		public Guid Trigger(Guid microService, string name, JObject e, IEventCallback callback)
		{
			var ev = Connection.GetService<IComponentService>().SelectComponent(microService, "Event", name);

			if (ev == null)
				throw new TomPITException(SR.ErrEventNotDefined);

			var u = Connection.CreateUrl("Event", "Trigger")
				.AddParameter("microService", microService)
				.AddParameter("name", name);

			if (callback != null)
				u.AddParameter("callback", string.Format("{0}/{1}/{2}", callback.MicroService, callback.Api, callback.Operation));

			return Connection.Post<Guid>(u, e);
		}
	}
}
