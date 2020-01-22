using System;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation : MiddlewareOperation, IOperation
	{
		public void Invoke()
		{
			Validate();
			DependencyInjections.Validate();

			try
			{
				OnAuthorize();
				DependencyInjections.Authorize();

				OnInvoke();
				DependencyInjections.Invoke<object>(null);

				if (IsCommitable)
				{
					OnCommit();
					DependencyInjections.Commit();
				}

				if (Context is MiddlewareContext mc)
					mc.CloseConnections();
			}
			catch (Exception ex)
			{
				throw new ScriptException(this, ex);
			}
		}

		protected virtual void OnInvoke()
		{
		}

		protected virtual void OnAuthorize()
		{
		}
	}
}
