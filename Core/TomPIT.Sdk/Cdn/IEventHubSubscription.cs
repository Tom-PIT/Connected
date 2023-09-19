using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn
{
	public enum EventSubscriptionBehavior
	{
		Reliable = 1,
		FireForget = 2
	}
	public interface IEventHubSubscription
	{
		string Name { get; }
		string Client { get; }
		string Recipient { get; }
		EventSubscriptionBehavior Behavior { get; }
		JObject Authorization { get; }
		JObject Arguments { get; }
	}
}
