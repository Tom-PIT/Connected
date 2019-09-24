using TomPIT.Collections;

namespace TomPIT.ComponentModel.Messaging
{
	public interface IEventBindingConfiguration : IConfiguration, ISourceCode
	{
		ListItems<IEventBinding> Events { get; }
	}
}
