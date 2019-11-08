using TomPIT.Collections;

namespace TomPIT.ComponentModel.Cdn
{
	public interface IDataHubEndpoint : IElement
	{
		string Name { get; }
		ListItems<IDataHubEndpointPolicy> Policies { get; }
	}
}
