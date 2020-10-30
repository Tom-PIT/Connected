using System;
using System.Runtime.ExceptionServices;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Distributed
{
	public abstract class DistributedEventMiddleware : MiddlewareOperation, IDistributedEventMiddleware
	{
		public void Invoke()
		{
			try
			{
				Context.Grant();
				Validate();
				OnInvoke();
				base.Invoked();
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException)
			{
				Rollback();
				throw;
			}
			catch (Exception ex)
			{
				Rollback();

				var se = new ScriptException(this, TomPITException.Unwrap(this, ex));

				ExceptionDispatchInfo.Capture(se).Throw();
			}
		}

		protected virtual void OnInvoke()
		{

		}

		public new void Invoked()
		{
			base.Invoked();
			OnInvoked();
		}

		protected virtual void OnInvoked()
		{

		}
		public bool Invoking()
		{
			Validate();
			return OnInvoking();
		}

		protected virtual bool OnInvoking()
		{
			return true;
		}

		public void Authorize(EventConnectionArgs e)
		{
			Context.Tenant.GetService<IAuthorizationService>().AuthorizePolicies(Context, this);
		}
	}
}
