using TomPIT.Collections;

namespace TomPIT.ComponentModel.Messaging
{
	public interface IEventHandlerConfiguration : IConfiguration, ISourceCode
	{
		ListItems<IEventBinding> Events { get; }
	}
}
