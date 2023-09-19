using System;
using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public interface IMicroServiceInfoMiddleware : IMiddlewareComponent
	{
		Version Version { get; }
		[Obsolete("Please use Contact property.")]
		string Author { get; }
		string Title { get; }
		string TermsOfService { get; }

		IMicroServiceContact Contact { get; }
		IMicroServiceLicense License { get; }

		string PrimaryDomain { get; }
		List<string> SecondaryDomains { get; }
		bool IsConnector { get; }
		bool IsExtender { get; }
		bool SupportsFrontEnd { get; }
		string Logo { get; }
		string Description { get; }
	}
}
