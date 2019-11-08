using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IIoCEndpointMiddleware : IMiddlewareObject
	{
		bool CanHandleRequest();
	}

	public interface IIoCEndpointMiddleware<R, A> : IIoCEndpointMiddleware
	{
		R Invoke(A e);
	}
	public interface IIoCEndpointMiddleware<A> : IIoCEndpointMiddleware
	{
		void Invoke(A e);
	}
}