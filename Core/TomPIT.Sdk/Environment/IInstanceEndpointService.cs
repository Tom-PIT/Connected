using System;
using System.Collections.Immutable;

namespace TomPIT.Environment
{
	public interface IInstanceEndpointService
	{
		IInstanceEndpoint Select(Guid endpoint);
		IInstanceEndpoint Select(InstanceType type);
		ImmutableList<IInstanceEndpoint> Query();
		ImmutableList<IInstanceEndpoint> Query(InstanceType type);

		string Url(InstanceType type, InstanceVerbs verb);
	}
}
