using TomPIT.Middleware;

namespace TomPIT.Data
{
	public interface IConnectionMiddleware : IMiddlewareComponent
	{
		IConnectionString Invoke();
	}
}
