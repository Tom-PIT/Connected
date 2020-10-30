using TomPIT.Collections;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IDistributedEventsConfiguration : IConfiguration, IText
	{
		ListItems<IDistributedEvent> Events { get; }
	}
}
