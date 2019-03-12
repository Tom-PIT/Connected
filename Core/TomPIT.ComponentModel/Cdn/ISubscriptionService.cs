using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn
{
	public interface ISubscriptionService
	{
		void CreateSubscription(ComponentModel.Cdn.ISubscription handler, string primaryKey, string topic);
		void TriggerEvent(ComponentModel.Cdn.ISubscription handler, string name, string primaryKey, string topic, JObject arguments);
	}
}
