using System;
using TomPIT.Configuration;
using TomPIT.Middleware;

namespace TomPIT.Reflection
{
	public interface IMicroServiceInfoDiscovery
	{
		IMicroServiceInfoMiddleware SelectMiddleware(IMicroServiceContext context, Guid microService);
	}
}
