using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public abstract class EventMiddleware : MiddlewareComponent, IEventMiddleware
	{
		public string EventName { get; set; }
		public bool Cancel { get; protected set; }

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
