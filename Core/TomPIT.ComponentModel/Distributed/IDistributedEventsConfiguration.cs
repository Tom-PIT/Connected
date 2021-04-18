using TomPIT.Collections;

namespace TomPIT.ComponentModel.Distributed
{
	public interface IDistributedEventsConfiguration : IConfiguration, IText, INamespaceElement
	{
		ListItems<IDistributedEvent> Events { get; }
	}
}
