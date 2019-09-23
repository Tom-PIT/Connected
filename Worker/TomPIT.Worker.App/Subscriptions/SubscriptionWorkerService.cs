using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Storage;
using TomPIT.Worker.Services;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionWorkerService : TenantObject, ISubscriptionWorkerService
	{
		public SubscriptionWorkerService(ITenant tenant) : base(tenant)
		{
		}

		public void CompleteSubscription(Guid popReceipt)
		{
			var url = Tenant.CreateUrl("SubscriptionManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Tenant.Post(url, d);
		}

		public void CompleteEvent(Guid popReceipt)
		{
			var url = Instance.Tenant.CreateUrl("SubscriptionManagement", "CompleteEvent");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Tenant.Post(url, d);
		}

		public void InsertSubscribers(Guid token, List<IRecipient> recipients)
		{
			if (recipients == null || recipients.Count == 0)
				return;

			var u = Instance.Tenant.CreateUrl("Subscription", "InsertSubscribers");
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

			Instance.Tenant.Post(u, e);
		}

		public void PingSubscription(Guid popReceipt)
		{
			var url = Instance.Tenant.CreateUrl("SubscriptionManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Tenant.Post(url, d);
		}

		public void PingSubscriptionEvent(Guid popReceipt)
		{
			var url = Instance.Tenant.CreateUrl("SubscriptionManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Tenant.Post(url, d);
		}

		public ISubscription SelectSubscription(Guid token)
		{
			var u = Instance.Tenant.CreateUrl("SubscriptionManagement", "Select");
			var e = new JObject
			{
				{"token",  token}
			};

			return Instance.Tenant.Post<SubscriptionDescriptor>(u, e);
		}

		public ISubscriptionEvent SelectEvent(Guid token)
		{
			var u = Instance.Tenant.CreateUrl("SubscriptionManagement", "SelectEvent");
			var e = new JObject
			{
				{"token",  token}
			};

			return Instance.Tenant.Post<SubscriptionEventDescriptor>(u, e);
		}

		public List<IRecipient> QueryRecipients(Guid subscription, string primaryKey, string topic)
		{
			var u = Instance.Tenant.CreateUrl("Subscription", "QuerySubscribers");
			var e = new JObject
			{
				{"handler",  subscription},
				{"primaryKey",  primaryKey}
			};

			if (!string.IsNullOrWhiteSpace(topic))
				e.Add("topic", topic);

			return Instance.Tenant.Post<List<Subscriber>>(u, e).ToList<IRecipient>();
		}

		public List<IQueueMessage> DequeueSubscriptions(string resourceGroup, int count)
		{
			var url = Instance.Tenant.CreateUrl("SubscriptionManagement", "Dequeue");

			var e = new JObject
				{
					{ "count", count },
					{ "resourceGroup", resourceGroup }
				};

			return Instance.Tenant.Post<List<QueueMessage>>(url, e).ToList<IQueueMessage>();
		}

		public List<IQueueMessage> DequeueSubscriptionEvents(string resourceGroup, int count)
		{
			var url = Instance.Tenant.CreateUrl("SubscriptionManagement", "DequeueEvents");

			var e = new JObject
				{
					{ "count", count },
					{ "resourceGroup", resourceGroup }
				};

			return Instance.Tenant.Post<List<QueueMessage>>(url, e).ToList<IQueueMessage>();
		}
	}
}
