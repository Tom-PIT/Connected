using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public interface IEventMiddleware : IMiddlewareComponent
	{
		string EventName { get; set; }
		bool Cancel { get; }
		void Invoke();
	}
}
