using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public abstract class DistributedEventMiddleware : MiddlewareOperation, IDistributedEventMiddleware
	{
		public void Invoke()
		{
			Validate();
			OnInvoke();
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
