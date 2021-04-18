using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Exceptions;
using TomPIT.Messaging;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareEvents : MiddlewareObject, IMiddlewareEvents
	{
		public Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name, [CIP(CIP.DistributedEventPropertyProvider)]object e)
		{
			return TriggerEvent(name, e, null);
		}

		public Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name)
		{
			return TriggerEvent(name, null);
		}

		public Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name, [CIP(CIP.DistributedEventPropertyProvider)]object e, IMiddlewareCallback callback)
		{
			if (callback is MiddlewareCallback ec)
				ec.Attached = true;

			IDistributedEvent ev = null;

			if (string.Compare(name, "$", true) != 0)
			{
				var config = ComponentDescriptor.DistributedEvent(Context, name);

				config.Validate();

				ev = config.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);

				if (ev == null)
					throw new RuntimeException($"{SR.ErrDistributedEventNotFound} ({name})");
			}

			return Context.Tenant.GetService<IEventService>().Trigger(ev, callback, e);
		}

		public Guid TriggerEvent([CIP(CIP.DistributedEventProvider)]string name, IMiddlewareCallback callback)
		{
			return TriggerEvent(name, null, callback);
		}
	}
}
