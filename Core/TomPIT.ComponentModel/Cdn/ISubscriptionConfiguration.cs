using TomPIT.Collections;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscriptionConfiguration : IConfiguration, IText, INamespaceElement
	{
		ListItems<ISubscriptionEvent> Events { get; }
	}
}
