using Newtonsoft.Json.Linq;
using TomPIT.Cdn;

namespace TomPIT.Data
{
	internal class EventHubSubscription : IEventHubSubscription
	{
		public string Name { get; set; }

		public JObject Authorization { get; set; }

		public JObject Arguments { get; set; }

		public string Client {get;set;}

		public EventSubscriptionBehavior Behavior {get;set;}
	}
}
