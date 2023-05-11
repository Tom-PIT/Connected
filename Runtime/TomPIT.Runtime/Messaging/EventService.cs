using System;
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
		public Guid Trigger(IDistributedEvent ev, IMiddlewareCallback callback)
		{
			return Trigger<object>(ev, callback, null);
		}

		public Guid Trigger<T>(IDistributedEvent ev, IMiddlewareCallback callback, T e)
		{
			var ms = Guid.Empty;
			var name = string.Empty;
			var cb = string.Empty;
			var args = string.Empty;

			if (ev is not null)
			{
				ms = ev.Configuration().MicroService();
				name = $"{ev.Configuration().ComponentName()}/{ev.Name}";
			}
			else
			{
				ms = callback.MicroService;
				name = "$";
			}

			if (callback is not null)
				cb = $"{callback.MicroService}/{callback.Component}/{callback.Element}";

			if (e is not null)
				args = Serializer.Serialize(e);

			return Instance.SysProxy.Events.Trigger(ms, name, cb, args);
		}
	}
}
