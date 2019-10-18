using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IIoCOperationMiddleware : IMiddlewareComponent
	{
	}

	public interface IIoCOperationMiddleware<A> : IIoCOperationMiddleware
	{
		void Invoke(A e);
		void Invoke(A e, List<IIoCEndpointMiddleware> endpoints);
		void Invoke(A e, IIoCEndpointMiddleware endpoint);
	}

	public interface IIoCContainerMiddleware<R, A> : IIoCOperationMiddleware
	{
		List<R> Invoke(A e);
		List<R> Invoke(A e, List<IIoCEndpointMiddleware> endpoints);
		R Invoke(A e, IIoCEndpointMiddleware endpoint);
	}
}
