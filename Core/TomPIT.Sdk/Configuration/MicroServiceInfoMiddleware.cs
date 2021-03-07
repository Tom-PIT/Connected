using System;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public abstract class MicroServiceInfoMiddleware : MiddlewareComponent, IMicroServiceInfoMiddleware
	{
		private Version _version = null;
		private IMicroServiceLicense _license = null;
		private IMicroServiceContact _contact = null;
		public virtual Version Version =>_version ??= new Version(0, 0, 0, 0);
		[Obsolete("Please use Contact property.")]
		public virtual string Author => null;

		public virtual IMicroServiceLicense License => _license ??= new MicroServiceLicense();
		public virtual IMicroServiceContact Contact => _contact ??= new MicroServiceContact();

		public virtual string Title => null;

		public virtual string TermsOfService => null;
	}
}
