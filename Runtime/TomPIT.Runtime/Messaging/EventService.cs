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
using TomPIT.Serialization;

namespace TomPIT.Messaging;

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
			RunLocally(ev, callback, e);

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

	private void RunLocally<T>(IDistributedEvent ev, IMiddlewareCallback callback, T e)
	{
		var ms = callback is null ? ev.Configuration().MicroService() : callback.MicroService;
		var disposeContext = false;
		IMicroServiceContext? ctx = null;
		IDistributedOperation? op = null;

		if (callback is MiddlewareCallback mc)
		{
			if (!mc.Context.IsActive())
				disposeContext = true;
			else
				ctx = mc.Context as MicroServiceContext;
		}

		if (ctx is null)
		{
			disposeContext = true;
			ctx ??= new MicroServiceContext(ms);
		}

		try
		{
			if (e is IDistributedOperation de)
				op = Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(ctx, de.GetType(), Serializer.Serialize(e));
			else
			{
				if (callback is null)
					return;

				var config = (Tenant.GetService<IComponentService>().SelectConfiguration(callback.Component) as IApiConfiguration)?.Operations.FirstOrDefault(f => f.Id == callback.Element);

				if (config is not null)
					op = Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(ctx, config, Serializer.Serialize(e), config.Name);
			}

			if (op is not null)
			{
				ReflectionExtensions.SetPropertyValue(op, nameof(IDistributedOperation.OperationTarget), DistributedOperationTarget.InProcess);

				op.Invoke();
			}
		}
		finally
		{
			if (disposeContext)
				ctx.Dispose();
		}
	}
}
