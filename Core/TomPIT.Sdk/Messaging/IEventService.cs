using System;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public interface IEventService
	{
		Guid Trigger<T>(IDistributedEvent ev, IMiddlewareCallback callback, T e);
		Guid Trigger(IDistributedEvent ev, IMiddlewareCallback callback);
	}
}
