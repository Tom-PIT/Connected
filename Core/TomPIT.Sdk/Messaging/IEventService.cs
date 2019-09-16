using System;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public interface IEventService
	{
		Guid Trigger<T>(IDistributedEventConfiguration ev, IMiddlewareCallback callback, T e);
		Guid Trigger(IDistributedEventConfiguration ev, IMiddlewareCallback callback);
	}
}
