using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public interface ICdnService
	{
		ICdnEventConnection Connect(IMiddlewareContext context);
	}
}
