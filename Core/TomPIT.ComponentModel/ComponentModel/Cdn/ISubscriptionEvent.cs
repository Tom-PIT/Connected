using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscriptionEvent : IConfigurationElement
	{
		string Name { get; }
		IServerEvent Invoke { get; }
	}
}
