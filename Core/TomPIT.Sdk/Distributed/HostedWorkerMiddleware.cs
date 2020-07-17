using System;
using System.Runtime.ExceptionServices;
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

				var se = new ScriptException(this, TomPITException.Unwrap(this, ex));

				ExceptionDispatchInfo.Capture(se).Throw();
			}
		}

		protected abstract void OnInvoke();
	}
}
