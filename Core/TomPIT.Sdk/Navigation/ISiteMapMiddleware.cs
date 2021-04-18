using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public interface ISiteMapMiddleware : IMiddlewareComponent
	{
		List<ISiteMapContainer> Invoke();

		List<INavigationContext> QueryContexts();
	}
}
