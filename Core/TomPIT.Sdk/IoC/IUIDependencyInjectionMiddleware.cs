using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IUIDependencyInjectionMiddleware : IMiddlewareObject
	{
		List<IUIDependencyDescriptor> Invoke();
	}
}
