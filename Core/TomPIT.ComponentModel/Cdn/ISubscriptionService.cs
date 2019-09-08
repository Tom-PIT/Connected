using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn
{
	public interface ISubscriptionService
	{
		bool SubscriptionExists(ComponentModel.Cdn.ISubscription handler, string primaryKey, string topic);
		void CreateSubscription(ComponentModel.Cdn.ISubscription handler, string primaryKey, string topic);
		void TriggerEvent<T>(ComponentModel.Cdn.ISubscription handler, string name, string primaryKey, string topic, T arguments);
	}
}
