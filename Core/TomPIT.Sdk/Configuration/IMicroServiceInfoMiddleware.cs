using System;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public interface IMicroServiceInfoMiddleware : IMiddlewareComponent
	{
		Version Version { get; }
		[Obsolete("Please use Contact property.")]
		string Author { get; }
		string Title { get;  }
		string TermsOfService { get; }

		IMicroServiceContact Contact { get; }
		IMicroServiceLicense License { get; }
	}
}
