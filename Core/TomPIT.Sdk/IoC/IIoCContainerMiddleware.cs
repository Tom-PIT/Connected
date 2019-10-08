using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IIoCContainerMiddleware : IMiddlewareComponent
	{
	}

	public interface IIoCContainerMiddleware<A> : IIoCContainerMiddleware
	{
		void Invoke(A e);
		void Invoke(A e, IIoCEndpointMiddleware endpoint);
	}

	public interface IIoCContainerMiddleware<R, A> : IIoCContainerMiddleware
	{
		List<R> Invoke(A e);
		R Invoke(A e, IIoCEndpointMiddleware endpoint);
	}
}
