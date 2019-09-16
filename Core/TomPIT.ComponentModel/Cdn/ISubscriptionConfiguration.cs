using TomPIT.Collections;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscriptionConfiguration : IConfiguration, ISourceCode
	{
		ListItems<ISubscriptionEvent> Events { get; }
	}
}
