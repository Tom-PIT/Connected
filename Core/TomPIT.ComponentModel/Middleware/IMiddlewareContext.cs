using TomPIT.ComponentModel;
using TomPIT.Middleware.Services;

namespace TomPIT.Middleware
{
	public interface IMiddlewareContext
	{
		IMiddlewareServices Services { get; }
		IMicroService MicroService { get; }
	}
}
