using System;
using System.Collections.Immutable;
using TomPIT.Environment;

namespace TomPIT.Proxy
{
	public interface IInstanceEndpointController
	{
		IInstanceEndpoint Select(Guid endpoint);
		ImmutableList<IInstanceEndpoint> Query();
	}
}
