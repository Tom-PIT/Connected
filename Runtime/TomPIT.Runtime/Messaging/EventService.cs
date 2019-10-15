using System;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Distributed;
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
			TriggerInvoking(ev, e);

			var u = Tenant.CreateUrl("Event", "Trigger");
			var args = new JObject
			{
				{"microService", ev.Configuration().MicroService() },
				{"name", $"{ev.Configuration().ComponentName()}/{ev.Name}" }
			};

			if (callback != null)
				args.Add("callback", $"{callback.MicroService}/{callback.Component}/{callback.Element}");

			if (e != null)
				args.Add("arguments", Serializer.Serialize(e));

			return Tenant.Post<Guid>(u, args);
		}

		private void TriggerInvoking<T>(IDistributedEvent configuration, T e)
		{
			var type = Tenant.GetService<ICompilerService>().ResolveType(configuration.Configuration().MicroService(), configuration, configuration.Name, false);

			if (type == null)
				return;

			var instance = Tenant.GetService<ICompilerService>().CreateInstance<IDistributedEventMiddleware>(new MicroServiceContext(configuration.Configuration().MicroService(), Tenant.Url), type, Serializer.Serialize(e));

			instance.Invoking();
		}
	}
}
