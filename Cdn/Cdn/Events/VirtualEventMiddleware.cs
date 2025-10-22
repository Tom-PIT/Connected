using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Events;

internal class VirtualEventMiddleware
	: DistributedEventMiddleware
{
	public VirtualEventMiddleware(IMiddlewareContext context)
	{
		Context = context;
	}

	protected override void OnInvoke()
	{

	}
}
