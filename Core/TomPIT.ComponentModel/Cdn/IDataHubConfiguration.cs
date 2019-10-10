using TomPIT.Collections;

namespace TomPIT.ComponentModel.Cdn
{
	public interface IDataHubConfiguration : IConfiguration
	{
		ListItems<IDataHubEndpoint> Endpoints { get; }
	}
}
