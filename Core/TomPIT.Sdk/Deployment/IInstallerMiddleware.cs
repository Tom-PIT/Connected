using TomPIT.Middleware;

namespace TomPIT.Deployment
{
	public interface IInstallerMiddleware : IMiddlewareComponent
	{
		void Invoke();
	}
}
