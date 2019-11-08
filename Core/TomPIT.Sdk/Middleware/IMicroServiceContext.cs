using TomPIT.ComponentModel;

namespace TomPIT.Middleware
{
	public interface IMicroServiceContext : IMiddlewareContext
	{
		IMicroService MicroService { get; }
	}
}
