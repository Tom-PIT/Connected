using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class QueueMiddleware : MiddlewareComponent, IQueueMiddleware
	{
		public virtual QueueValidationBehavior ValidationFailed => QueueValidationBehavior.Retry;

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
