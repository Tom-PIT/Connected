using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public interface IDistributedEventMiddleware : IMiddlewareComponent
	{
		bool Invoking();
		void Invoke();
		void Invoked();
	}
}
