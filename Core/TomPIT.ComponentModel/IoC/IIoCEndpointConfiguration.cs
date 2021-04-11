using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IIoCEndpointConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IIoCEndpoint> Endpoints { get; }
	}
}
