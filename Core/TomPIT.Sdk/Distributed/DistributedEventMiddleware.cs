using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public abstract class DistributedEventMiddleware : MiddlewareComponent, IDistributedEventMiddleware
	{
		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}

		public void Invoked()
		{
			OnInvoked();
		}

		protected virtual void OnInvoked()
		{

		}
		public void Invoking()
		{
			Validate();
			OnInvoking();
		}

		protected virtual void OnInvoking()
		{

		}
	}
}
