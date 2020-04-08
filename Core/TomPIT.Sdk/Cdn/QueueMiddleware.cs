using System;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class QueueMiddleware : MiddlewareOperation, IQueueMiddleware
	{
		public virtual QueueValidationBehavior ValidationFailed => QueueValidationBehavior.Retry;

		public void Invoke()
		{
			Invoke(null);
		}
		public void Invoke(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			Validate();

			try
			{
				OnInvoke();

				Invoked();
			}
			catch (Exception ex)
			{
				Rollback();
				throw new ScriptException(this, ex);
			}
		}

		protected abstract void OnInvoke();
	}
}
