using System;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public abstract class MicroServiceInfoMiddleware : MiddlewareComponent, IMicroServiceInfoMiddleware
	{
		private Version _version = null;
		private IMicroServiceLicense _license = null;
		private IMicroServiceContact _contact = null;
		public virtual Version Version => _version ??= new Version(0, 0, 0, 0);
		[Obsolete("Please use Contact property.")]
		public virtual string Author => null;

		public virtual IMicroServiceLicense License => _license ??= new MicroServiceLicense();
		public virtual IMicroServiceContact Contact => _contact ??= new MicroServiceContact();

		public virtual string Title { get; protected set; }

		public virtual string TermsOfService { get; protected set; }

		public virtual string PrimaryDomain { get; protected set; }

		public virtual string SecondaryDomain { get; protected set; }

		public virtual bool IsConnector { get; protected set; }

		public virtual bool IsExtender { get; protected set; }

		public virtual bool SupportsFrontEnd { get; protected set; }

		public virtual string Logo { get; protected set; }
		public virtual string Description { get; protected set; }
	}
}
