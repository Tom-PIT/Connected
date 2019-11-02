using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IIoCEndpointConfiguration : IConfiguration
	{
		ListItems<IIoCEndpoint> Endpoints { get; }
	}
}
