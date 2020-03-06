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
					Commit();
					//OnCommit();
					DependencyInjections.Commit();
				}

				if (Context is MiddlewareContext mc)
					mc.CloseConnections();

				if (IsCommitable)
				{
					if (Transaction is MiddlewareTransaction t)
						t.Complete();
				}

				if (!IsCommitted)
					OnCommit();
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException)
			{
				throw;
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
