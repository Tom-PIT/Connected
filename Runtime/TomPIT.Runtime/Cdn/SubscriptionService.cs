using System;
using System.Collections.Generic;
using System.Linq;
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
				e.Add("arguments", Serializer.Serialize(arguments));

			Tenant.Post(u, e);
		}

		public List<IRecipient> QuerySubscribers(ISubscriptionConfiguration configuration, string primaryKey)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "QuerySubscribers");
			var e = new JObject
			{
				{"handler",  configuration.Component},
				{"primaryKey",  primaryKey}
			};

			return MiddlewareDescriptor.Current.Tenant.Post<List<Recipient>>(u, e).ToList<IRecipient>();
		}

		public IRecipient SelectSubscriber(Guid token)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "SelectSubscriberByToken");
			var e = new JObject
			{
				{"token",  token}
			};

			return MiddlewareDescriptor.Current.Tenant.Post<Recipient>(u, e);
		}

		public IRecipient SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "SelectSubscriberBySubscription");
			var e = new JObject
			{
				{"subscription",  subscription},
				{"type",  type.ToString()},
				{"resourcePrimaryKey",  resourcePrimaryKey}
			};

			return MiddlewareDescriptor.Current.Tenant.Post<Recipient>(u, e);
		}

		public Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "InsertSubscriber");

			return MiddlewareDescriptor.Current.Tenant.Post<Guid>(u, new
			{
				subscription,
				type,
				resourcePrimaryKey,
				tags
			});
		}

		public void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "DeleteSubscriber");
			var e = new JObject
			{
				{"subscription",  subscription},
				{"type",  type.ToString()},
				{"resourcePrimaryKey",  resourcePrimaryKey}
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void InsertSubscribers(Guid subscription, List<IRecipient> recipients)
		{
			if (recipients is null || !recipients.Any())
				return;

			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "InsertSubscribers");
			var e = new JObject
				{
					{"subscription", subscription }
				};

			var items = new List<object>();

			foreach (var recipient in recipients)
			{
				items.Add(new
				{
					recipient.Type,
					recipient.ResourcePrimaryKey,
					recipient.Tags
				});
			}

			MiddlewareDescriptor.Current.Tenant.Post(u, new
			{
				subscription,
				items
			});
		}

		public ISubscription SelectSubscription(Guid token)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "SelectSubscription");
			var e = new JObject
			{
				{"token",  token}
			};

			return MiddlewareDescriptor.Current.Tenant.Post<SubscriptionDescriptor>(u, e);
		}

		public ISubscription SelectSubscription(Guid configuration, string primaryKey)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("Subscription", "SelectSubscriptionByConfiguration");
			var e = new JObject
			{
				{"handler",  configuration},
				{"primaryKey",  primaryKey}
			};

			return MiddlewareDescriptor.Current.Tenant.Post<SubscriptionDescriptor>(u, e);
		}
	}
}
