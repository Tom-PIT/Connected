using Newtonsoft.Json.Linq;

namespace TomPIT.Cdn.Events
{
	public class EventHubSubscription : IEventHubSubscription
	{
		public string Name { get; set; }

		public JObject Authorization {get;set;}

		public JObject Arguments {get;set;}

		public string Client {get;set;}
		public string Recipient { get; set; }

		public EventSubscriptionBehavior Behavior { get; set; } = EventSubscriptionBehavior.Reliable;
	}
}
