using System;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Events;

internal sealed class EventBindingDescriptor : IEventBindingDescriptor
{
	public bool IsBound(IDistributedEvent e)
	{
		var compiler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>();
		var ms = Tenant.GetService<IMicroServiceService>().Select(e.Configuration().MicroService());
		using var ctx = new MicroServiceContext(ms.Token);

		if (compiler.ResolveType(ms.Token, e, e.Name, false) is Type type)
			return true;

		var eventName = $"{ms.Name}/{e.Configuration().ComponentName()}/{e.Name}".ToLowerInvariant();
		var handlers = EventHandlers.Query(eventName);

		if (handlers is not null && handlers.Count != 0)
			return true;

		var clients = EventClients.Query(eventName);

		if (clients is not null && clients.Count != 0)
			return true;

		return false;
	}
}
