using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public interface IEventMiddleware : IMiddlewareComponent
	{
		string EventName { get; }
		bool Cancel { get; }
		void Invoke();
	}
}
