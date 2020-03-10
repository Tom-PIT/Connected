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
			Invoked();
		}

		protected abstract void OnInvoke();

		protected internal void Invoked()
		{
			var mc = Context as MiddlewareContext;

			if (mc?.Owner == null)
			{
				if (!(mc.Transaction is MiddlewareTransaction transaction))
					return;

				transaction.Commit();
			}
		}
	}
}
