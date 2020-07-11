using TomPIT.Middleware;

namespace TomPIT.Runtime
{
	public abstract class RuntimeMiddleware : MiddlewareObject, IRuntimeMiddleware
	{
		public void Initialize(RuntimeInitializeArgs e)
		{
			OnInitialize(e);
		}

		protected virtual void OnInitialize(RuntimeInitializeArgs e)
		{

		}
	}
}
