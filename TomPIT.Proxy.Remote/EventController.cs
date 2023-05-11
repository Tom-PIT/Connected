using System;
using System.Collections.Generic;

namespace TomPIT.Proxy.Remote
{
	internal class EventController : IEventController
	{
		private const string Controller = "Event";
		public Guid Trigger(Guid microService, string name, string callback, string arguments)
		{
			var u = Connection.CreateUrl(Controller, "Trigger");
			var args = new Dictionary<string, object>
			{
				{nameof(name), name },
				{nameof(microService), microService }
			};

			if (!string.IsNullOrWhiteSpace(callback))
				args.Add(nameof(callback), callback);

			if (!string.IsNullOrWhiteSpace(arguments))
				args.Add(nameof(arguments), arguments);

			return Connection.Post<Guid>(u, args);
		}
	}
}
