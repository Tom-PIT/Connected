using TomPIT.ComponentModel.Distributed;

namespace TomPIT.Cdn;
public interface IEventBindingDescriptor
{
	bool IsBound(IDistributedEvent e);
}
