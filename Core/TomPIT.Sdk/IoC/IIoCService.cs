using System.Collections.Generic;
using TomPIT.ComponentModel.IoC;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IIoCService
	{
		R Invoke<R>(IMiddlewareContext context, IIoCOperation operation, object e = null);

		void Invoke(IMiddlewareContext context, IIoCOperation operation, object e = null);
		List<IIoCEndpointMiddleware> CreateEndpoints(IMiddlewareContext context, IIoCOperation operation, object e);
		bool HasEndpoints(IMiddlewareContext context, IIoCOperation sender, object e);
	}
}
