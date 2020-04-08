using System;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public abstract class HostedWorkerMiddleware : MiddlewareOperation, IHostedWorkerMiddleware
	{
		public void Invoke()
		{
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
