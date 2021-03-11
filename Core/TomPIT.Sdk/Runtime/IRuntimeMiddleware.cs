using TomPIT.Middleware;

namespace TomPIT.Runtime
{
	public interface IRuntimeMiddleware : IMiddlewareObject
	{
		void Initialize(RuntimeInitializeArgs e);

		IRuntimeUrl ResolveUrl(RuntimeUrlKind kind);
	}
}
