using System.Collections.Generic;

namespace TomPIT.Cdn
{
	public interface IDataHubEndpointSubscriber
	{
		string Name { get; }
		List<IDataHubEndpointPolicySubscriber> Policies { get; }
	}
}
