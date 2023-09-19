using System;
using System.Collections.Immutable;

namespace TomPIT.Environment
{
	public interface IInstanceEndpointService
	{
		IInstanceEndpoint Select(Guid endpoint);
		IInstanceEndpoint Select(InstanceFeatures features);
		ImmutableList<IInstanceEndpoint> Query();
		ImmutableList<IInstanceEndpoint> Query(InstanceFeatures features);

		string Url(InstanceFeatures features, InstanceVerbs verb);
	}
}
