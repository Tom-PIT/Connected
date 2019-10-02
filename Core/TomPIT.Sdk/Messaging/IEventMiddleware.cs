using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public interface IEventMiddleware : IMiddlewareComponent
	{
		bool Cancel { get; }
		void Invoke(string eventName);
	}
}
