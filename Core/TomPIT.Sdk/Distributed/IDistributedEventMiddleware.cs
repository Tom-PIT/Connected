using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public interface IDistributedEventMiddleware : IMiddlewareComponent
	{
		void Invoking();
		void Invoke();
		void Invoked();
	}
}
