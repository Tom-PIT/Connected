using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public abstract class HostedWorkerMiddleware : MiddlewareComponent, IHostedWorkerMiddleware
	{
		public HostedWorkerMiddleware(IMiddlewareContext context) : base(context)
		{

		}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
