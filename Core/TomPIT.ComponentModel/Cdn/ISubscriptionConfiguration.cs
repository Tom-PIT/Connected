using TomPIT.Collections;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscriptionConfiguration : IConfiguration, IText
	{
		ListItems<ISubscriptionEvent> Events { get; }
	}
}
