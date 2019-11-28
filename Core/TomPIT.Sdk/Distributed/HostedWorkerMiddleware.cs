using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public abstract class HostedWorkerMiddleware : MiddlewareComponent, IHostedWorkerMiddleware
	{
		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
