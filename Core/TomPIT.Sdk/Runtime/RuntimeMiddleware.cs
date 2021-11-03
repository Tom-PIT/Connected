using System;
using TomPIT.Middleware;

namespace TomPIT.Runtime
{
	public abstract class RuntimeMiddleware : MiddlewareObject, IRuntimeMiddleware
	{
		public virtual IRuntimeResolver Resolver => null;

		public virtual IRuntimeViewModifier ViewModifier => null;

        public void Initialize(RuntimeInitializeArgs e)
		{
			OnInitialize(e);
		}

		public IRuntimeUrl ResolveUrl(RuntimeUrlKind kind)
		{
			return OnResolveUrl(kind);
		}
		[Obsolete("Please use Resolver.ResolveUrl instead.")]
		protected virtual IRuntimeUrl OnResolveUrl(RuntimeUrlKind kind)
		{
			return null;
		}

		protected virtual void OnInitialize(RuntimeInitializeArgs e)
		{

		}
	}
}
