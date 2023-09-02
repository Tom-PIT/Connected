using System;
using System.ComponentModel.DataAnnotations;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation : MiddlewareApiOperation, IOperation
	{
		public void Invoke()
		{
			Invoke(null);
		}

		public void Invoke(IMiddlewareContext? context)
		{
			if (context is not null)
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

				throw TomPITException.Unwrap(this, ex);
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
