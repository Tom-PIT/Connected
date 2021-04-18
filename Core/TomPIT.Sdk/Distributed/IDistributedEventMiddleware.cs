using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public enum EventInvokingResult
	{
		Cancel = 1,
		Delay = 2,
		Continue = 3
	}
	public interface IDistributedEventMiddleware : IMiddlewareComponent
	{
		//bool Invoking();
		void Invoking(DistributedEventInvokingArgs e);
		void Invoke();
		void Invoked();

		void Authorize(EventConnectionArgs e);
	}
}
