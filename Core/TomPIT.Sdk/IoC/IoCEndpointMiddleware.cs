using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public abstract class IoCEndpointMiddleware : MiddlewareObject, IIoCEndpointMiddleware
	{
		public bool CanHandleRequest()
		{
			return OnCanHandleRequest();
		}

		protected virtual bool OnCanHandleRequest()
		{
			return true;
		}
	}

	public abstract class IoCEndpointMiddleware<A> : IoCEndpointMiddleware, IIoCEndpointMiddleware<A>
	{
		public void Invoke(A e)
		{
			OnInvoke(e);
		}
		protected virtual void OnInvoke(A e)
		{

		}
	}

	public abstract class IoCEndpointMiddleware<R, A> : IoCEndpointMiddleware, IIoCEndpointMiddleware<R, A>
	{
		public R Invoke(A e)
		{
			return OnInvoke(e);
		}

		protected virtual R OnInvoke(A e)
		{
			return default;
		}
	}
}
