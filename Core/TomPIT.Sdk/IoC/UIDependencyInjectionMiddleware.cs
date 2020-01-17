using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public abstract class UIDependencyInjectionMiddleware : MiddlewareObject, IUIDependencyInjectionMiddleware
	{
		public List<IUIDependencyDescriptor> Invoke()
		{
			return OnInvoke();
		}

		protected virtual List<IUIDependencyDescriptor> OnInvoke()
		{
			return null;
		}
	}
}
