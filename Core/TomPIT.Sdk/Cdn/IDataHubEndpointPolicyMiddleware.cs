using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public interface IDataHubEndpointPolicyMiddleware<T> : IMiddlewareComponent
	{
		bool Invoke(T e);
	}
}
