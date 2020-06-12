using System;
using TomPIT.Exceptions;
using TomPIT.Middleware;

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
				Invoked();
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException)
			{
				Rollback();
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
	}
}
