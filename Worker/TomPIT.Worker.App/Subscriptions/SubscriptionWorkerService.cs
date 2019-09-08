using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.Storage;
using TomPIT.Worker.Services;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionWorkerService : ServiceBase, ISubscriptionWorkerService
	{
		public SubscriptionWorkerService(ISysConnection connection) : base(connection)
		{
		}

		public void CompleteSubscription(Guid popReceipt)
		{
			var url = Connection.CreateUrl("SubscriptionManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		public void CompleteEvent(Guid popReceipt)
		{
			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "CompleteEvent");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		public void InsertSubscribers(Guid token, List<IRecipient> recipients)
		{
			if (recipients == null || recipients.Count == 0)
				return;

			var u = Instance.Connection.CreateUrl("Subscription", "InsertSubscribers");
			var e = new JObject
				{
					{"subscription", token }
				};

			var a = new JArray();

			e.Add("items", a);

			foreach (var recipient in recipients)
			{
				a.Add(new JObject
					{
						{"type", recipient.Type.ToString() },
						{"resourcePrimaryKey", recipient.ResourcePrimaryKey }
					});
			}

			Instance.Connection.Post(u, e);
		}

		public void PingSubscription(Guid popReceipt)
		{
			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		public void PingSubscriptionEvent(Guid popReceipt)
		{
			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		public ISubscription SelectSubscription(Guid token)
		{
			var u = Instance.Connection.CreateUrl("SubscriptionManagement", "Select");
			var e = new JObject
			{
				{"token",  token}
			};

			return Instance.Connection.Post<SubscriptionDescriptor>(u, e);
		}

		public ISubscriptionEvent SelectEvent(Guid token)
		{
			var u = Instance.Connection.CreateUrl("SubscriptionManagement", "SelectEvent");
			var e = new JObject
			{
				{"token",  token}
			};

			return Instance.Connection.Post<SubscriptionEventDescriptor>(u, e);
		}

		public List<IRecipient> QueryRecipients(Guid subscription, string primaryKey, string topic)
		{
			var u = Instance.Connection.CreateUrl("Subscription", "QuerySubscribers");
			var e = new JObject
			{
				{"handler",  subscription},
				{"primaryKey",  primaryKey}
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			return Instance.Connection.Post<List<Subscriber>>(u, e).ToList<IRecipient>();
		}

		public List<IQueueMessage> DequeueSubscriptions(string resourceGroup, int count)
		{
			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "Dequeue");

			var e = new JObject
				{
					{ "count", count },
					{ "resourceGroup", resourceGroup }
				};

			return Instance.Connection.Post<List<QueueMessage>>(url, e).ToList<IQueueMessage>();
		}

		public List<IQueueMessage> DequeueSubscriptionEvents(string resourceGroup, int count)
		{
			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "DequeueEvents");

			var e = new JObject
				{
					{ "count", count },
					{ "resourceGroup", resourceGroup }
				};

			return Instance.Connection.Post<List<QueueMessage>>(url, e).ToList<IQueueMessage>();
		}
	}
}
