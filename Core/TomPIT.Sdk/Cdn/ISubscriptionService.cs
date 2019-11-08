using TomPIT.ComponentModel.Cdn;

namespace TomPIT.Cdn
{
	public interface ISubscriptionService
	{
		bool SubscriptionExists(ISubscriptionConfiguration configuration, string primaryKey, string topic);
		void CreateSubscription(ISubscriptionConfiguration configuration, string primaryKey, string topic);
		void TriggerEvent<T>(ISubscriptionConfiguration configuration, string name, string primaryKey, string topic, T arguments);
	}
}
