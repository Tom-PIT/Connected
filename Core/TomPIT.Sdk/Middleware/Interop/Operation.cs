using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation : MiddlewareApiOperation, IOperation
	{
		public void Invoke()
		{
			Invoke(null);
		}
		public void Invoke(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			try
			{
				Validate();
				OnValidating();

				if (Context.Environment.IsInteractive)
				{
					AuthorizePolicies();
					OnAuthorize();
					OnAuthorizing();
				}

				OnInvoke();
				DependencyInjections.Invoke<object>(null);

				Invoked();
			}
			catch (ValidationException)
			{
				Rollback();
				throw;
			}
			catch (Exception ex)
			{
				Rollback();

				var unwrapped = TomPITException.Unwrap(this, ex);

				if (unwrapped is ValidationException)
				{
					ExceptionDispatchInfo.Capture(unwrapped).Throw();
					throw;
				}
				else
				{
					var se = new ScriptException(this, unwrapped);

					ExceptionDispatchInfo.Capture(se).Throw();

					throw;
				}

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
