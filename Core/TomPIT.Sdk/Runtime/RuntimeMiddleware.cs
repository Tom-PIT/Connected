using TomPIT.Middleware;

namespace TomPIT.Runtime
{
	public abstract class RuntimeMiddleware : MiddlewareObject, IRuntimeMiddleware
	{
		public void Initialize(RuntimeInitializeArgs e)
		{
			OnInitialize(e);
		}

		public IRuntimeUrl ResolveUrl(RuntimeUrlKind kind)
		{
			return OnResolveUrl(kind);
		}

		protected virtual IRuntimeUrl OnResolveUrl(RuntimeUrlKind kind)
		{
			return null;
		}

		protected virtual void OnInitialize(RuntimeInitializeArgs e)
		{

		}
	}
}
