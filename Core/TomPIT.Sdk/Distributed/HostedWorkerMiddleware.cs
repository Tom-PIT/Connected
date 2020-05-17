using System;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public abstract class HostedWorkerMiddleware : MiddlewareOperation, IHostedWorkerMiddleware
	{
		public void Invoke()
		{
			Invoke(null);
		}
		public void Invoke(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			Context.Grant();
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
