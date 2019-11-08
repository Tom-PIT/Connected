using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IIoCOperationMiddleware : IMiddlewareComponent
	{
		void Invoke();
	}

	public interface IIoCOperationMiddleware<R> : IMiddlewareComponent
	{
		R Invoke();
	}
}
