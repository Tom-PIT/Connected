using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class DataHubEndpointPolicyMiddleware<T> : MiddlewareComponent, IDataHubEndpointPolicyMiddleware<T>
	{
		public bool Invoke(T e)
		{
			return OnInvoke(e);
		}

		protected virtual bool OnInvoke(T e)
		{
			return false;
		}
	}
}
