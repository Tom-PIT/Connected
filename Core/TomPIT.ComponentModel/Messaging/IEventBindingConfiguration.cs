using TomPIT.Collections;

namespace TomPIT.ComponentModel.Messaging
{
	public interface IEventBindingConfiguration : IConfiguration
	{
		ListItems<IEventBinding> Events { get; }
	}
}
