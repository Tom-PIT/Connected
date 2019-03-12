using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscription : IConfiguration
	{
		IServerEvent Subscribe { get; }
		IServerEvent Subscribed { get; }

		ListItems<ISubscriptionEvent> Events { get; }
	}
}
