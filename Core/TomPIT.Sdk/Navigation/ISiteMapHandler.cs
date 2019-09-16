using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public interface ISiteMapHandler : IMiddlewareComponent
	{
		List<ISiteMapContainer> Invoke(params string[] key);
	}
}
