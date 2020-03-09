using System;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation : MiddlewareOperation, IOperation
	{
		public void Invoke()
		{
			Validate();
			OnValidateDependencies();

			try
			{
				OnAuthorize();
				OnAuthorizeDependencies();

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
