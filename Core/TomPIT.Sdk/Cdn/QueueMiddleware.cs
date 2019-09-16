using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class QueueMiddleware : MiddlewareComponent, IQueueMiddleware
	{
		public QueueMiddleware(IMiddlewareContext context) : base(context)
		{
		}

		public virtual QueueValidationBehavior ValidationFailed => QueueValidationBehavior.Retry;

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
