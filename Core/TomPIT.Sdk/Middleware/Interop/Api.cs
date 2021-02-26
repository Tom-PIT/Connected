using System;

namespace TomPIT.Middleware.Interop
{
	public abstract class Api : MiddlewareObject, IApi
	{
		private Version _version;
		public virtual Version Version => _version ??= new Version();
	}
}
