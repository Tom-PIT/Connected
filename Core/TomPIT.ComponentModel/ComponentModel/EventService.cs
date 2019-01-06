using System;
using Newtonsoft.Json.Linq;
using TomPIT.Exceptions;
using TomPIT.Net;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	internal class EventService : IEventService
	{
		public EventService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; set; }

		public Guid Trigger(Guid microService, string name, JObject e, IEventCallback callback)
		{
			var ev = Server.GetService<IComponentService>().SelectComponent(microService, "Event", name);

			if (ev == null)
				throw new TomPITException(SR.ErrEventNotDefined);

			var u = Server.CreateUrl("Event", "Trigger")
				.AddParameter("microService", microService)
				.AddParameter("name", name);

			if (callback != null)
				u.AddParameter("callback", string.Format("{0}/{1}/{2}", callback.MicroService, callback.Api, callback.Operation));

			return Server.Connection.Post<Guid>(u, e);
		}
	}
}
