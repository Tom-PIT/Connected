using System;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Messaging
{
	internal class EventService : TenantObject, IEventService
	{
		public EventService(ITenant tenant) : base(tenant)
		{
		}
		public Guid Trigger(IDistributedEventConfiguration ev, IMiddlewareCallback callback)
		{
			return Trigger<object>(ev, callback, null);
		}

		public Guid Trigger<T>(IDistributedEventConfiguration ev, IMiddlewareCallback callback, T e)
		{
			var u = Tenant.CreateUrl("Event", "Trigger");
			var args = new JObject
			{
				{"microService", ev.MicroService() },
				{"name", ev.ComponentName() }
			};

			if (callback != null)
				args.Add("callback", $"{callback.MicroService}/{callback.Component}/{callback.Element}");

			if (e != null)
				args.Add("arguments", Serializer.Serialize(e));

			return Tenant.Post<Guid>(u, args);
		}
	}
}
