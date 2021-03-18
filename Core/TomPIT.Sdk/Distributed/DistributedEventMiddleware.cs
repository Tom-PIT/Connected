using System;
using System.Runtime.ExceptionServices;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Distributed
{
	public enum EventStage
	{
		Broadcasting = 1,
		Completing = 2
	}
	public abstract class DistributedEventMiddleware : MiddlewareOperation, IDistributedEventMiddleware
	{
		protected EventStage EventStage { get; private set; } = EventStage.Broadcasting;

		public void Invoke()
		{
			try
			{
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
			EventStage = EventStage.Completing;

			OnInvoked();
			base.Invoked();
		}

		protected virtual void OnInvoked()
		{

		}
		[Obsolete("Please use Invoking(e) instead.")]
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
			Context.Tenant.GetService<IAuthorizationService>().AuthorizePolicies(Context, e.Proxy ?? this);
		}

		public void Invoking(DistributedEventInvokingArgs e)
		{
			OnInvoking();
		}

		protected virtual void OnInvoking(DistributedEventInvokingArgs e)
		{

		}
	}
}
