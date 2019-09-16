using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public abstract class EventMiddleware : MiddlewareComponent, IEventMiddleware
	{
		protected EventMiddleware(IMiddlewareContext context, string eventName) : base(context)
		{
			EventName = eventName;
		}

		public string EventName { get; }
		public bool Cancel { get; protected set; }

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
