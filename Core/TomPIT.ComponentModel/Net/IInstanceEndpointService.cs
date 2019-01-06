using System;
using System.Collections.Generic;

namespace TomPIT.Net
{
	public interface IInstanceEndpointService
	{
		IInstanceEndpoint Select(Guid endpoint);
		IInstanceEndpoint Select(InstanceType type);
		List<IInstanceEndpoint> Query();
		List<IInstanceEndpoint> Query(InstanceType type);

		string Url(InstanceType type, InstanceVerbs verb);
	}
}
