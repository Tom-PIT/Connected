using System;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
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
			if (ev is not null && CdnUtils.BindingDescriptor is not null && !CdnUtils.BindingDescriptor.IsBound(ev))
			{
				if (e is IDistributedOperation de)
				{
					ReflectionExtensions.SetPropertyValue(de, nameof(IDistributedOperation.OperationTarget), DistributedOperationTarget.InProcess);

					de.Invoke();
				}
				else if (callback is MiddlewareCallback mc)
				{
					var op = (Tenant.GetService<IComponentService>().SelectConfiguration(callback.Component) as IApiConfiguration)?.Operations.FirstOrDefault(f => f.Id == callback.Element);

					if (op is not null)
					{
						var instance = Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(op, Serializer.Serialize(e), op.Name);

						if (instance is not null)
						{
							ReflectionExtensions.SetPropertyValue(instance, nameof(IDistributedOperation.OperationTarget), DistributedOperationTarget.InProcess);

							instance.SetContext(mc.Context);
							instance.Invoke();
						}
					}
				}

				return Guid.Empty;
			}

			Guid ms;
			string name;
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
