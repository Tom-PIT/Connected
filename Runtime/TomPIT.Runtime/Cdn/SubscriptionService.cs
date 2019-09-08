using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	internal class SubscriptionService : ServiceBase, ISubscriptionService
	{
		public SubscriptionService(ISysConnection connection) : base(connection)
		{
		}

		public bool SubscriptionExists(ComponentModel.Cdn.ISubscription handler, string primaryKey, string topic)
		{
			var u = Connection.CreateUrl("Subscription", "Exists");
			var e = new JObject
			{
				{ "handler", handler.Component },
				{ "primaryKey",primaryKey }
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			return Connection.Post<bool>(u, e);
		}

		public void CreateSubscription(ComponentModel.Cdn.ISubscription handler, string primaryKey, string topic)
		{
			var u = Connection.CreateUrl("Subscription", "Enqueue");
			var e = new JObject
			{
				{ "microService",handler.MicroService(Connection) },
				{ "handler", handler.Component },
				{ "primaryKey",primaryKey }
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			Connection.Post(u, e);
		}

		public void TriggerEvent<T>(ComponentModel.Cdn.ISubscription handler, string eventName, string primaryKey, string topic, T arguments)
		{
			var u = Connection.CreateUrl("Subscription", "EnqueueEvent");
			var e = new JObject
			{
				{ "microService",handler.MicroService(Connection) },
				{ "handler",handler.Component },
				{ "primaryKey",primaryKey },
				{ "name",eventName }
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			if (arguments != null)
				e.Add("arguments", Types.Serialize(arguments));

			Connection.Post(u, e);
		}
	}
}
