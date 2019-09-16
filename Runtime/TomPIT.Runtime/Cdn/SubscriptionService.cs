using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Cdn
{
	internal class SubscriptionService : TenantObject, ISubscriptionService
	{
		public SubscriptionService(ITenant tenant) : base(tenant)
		{
		}

		public bool SubscriptionExists(ISubscriptionConfiguration configuration, string primaryKey, string topic)
		{
			var u = Tenant.CreateUrl("Subscription", "Exists");
			var e = new JObject
			{
				{ "handler", configuration.Component },
				{ "primaryKey",primaryKey }
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			return Tenant.Post<bool>(u, e);
		}

		public void CreateSubscription(ISubscriptionConfiguration configuration, string primaryKey, string topic)
		{
			var u = Tenant.CreateUrl("Subscription", "Enqueue");
			var e = new JObject
			{
				{ "microService",configuration.MicroService() },
				{ "handler", configuration.Component },
				{ "primaryKey",primaryKey }
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			Tenant.Post(u, e);
		}

		public void TriggerEvent<T>(ISubscriptionConfiguration configuration, string eventName, string primaryKey, string topic, T arguments)
		{
			var u = Tenant.CreateUrl("Subscription", "EnqueueEvent");
			var e = new JObject
			{
				{ "microService",configuration.MicroService() },
				{ "handler",configuration.Component },
				{ "primaryKey",primaryKey },
				{ "name",eventName }
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			if (arguments != null)
				e.Add("arguments", SerializationExtensions.Serialize(arguments));

			Tenant.Post(u, e);
		}
	}
}
