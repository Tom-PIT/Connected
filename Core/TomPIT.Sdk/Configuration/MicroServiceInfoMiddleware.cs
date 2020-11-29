using System;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public abstract class MicroServiceInfoMiddleware : MiddlewareComponent, IMicroServiceInfoMiddleware
	{
		private Version _version = null;
		public virtual Version Version { get { return _version ??= new Version(0, 0, 0, 0); } }
		public virtual string Author => null;
	}
}
