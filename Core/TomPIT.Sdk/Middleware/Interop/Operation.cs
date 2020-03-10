using System;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation : MiddlewareApiOperation, IOperation
	{
		public void Invoke()
		{
			Validate();
			OnValidating();

			try
			{
				OnAuthorize();
				OnAuthorizing();

				OnInvoke();
				DependencyInjections.Invoke<object>(null);

				Invoked();
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				Rollback();
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
