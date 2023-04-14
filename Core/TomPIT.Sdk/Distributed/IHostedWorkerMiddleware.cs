using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public interface IHostedWorkerMiddleware : IMiddlewareComponent, ILifetimeMiddleware
	{
		void Invoke();
		void Invoke(IMiddlewareContext context);
	}
}
