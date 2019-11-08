using TomPIT.Collections;

namespace TomPIT.ComponentModel.Messaging
{
	public interface IEventBindingConfiguration : IConfiguration, IText
	{
		ListItems<IEventBinding> Events { get; }
	}
}
