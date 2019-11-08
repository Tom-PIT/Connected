using TomPIT.Collections;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IDistributedEventsConfiguration : IConfiguration
	{
		ListItems<IDistributedEvent> Events { get; }
	}
}
