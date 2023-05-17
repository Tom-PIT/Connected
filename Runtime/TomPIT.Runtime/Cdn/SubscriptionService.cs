using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;
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
			return Instance.SysProxy.Subscriptions.SubscriptionExists(configuration.Component, primaryKey, topic);
		}

		public void CreateSubscription(ISubscriptionConfiguration configuration, string primaryKey, string topic)
		{
			Instance.SysProxy.Subscriptions.CreateSubscription(configuration.MicroService(), configuration.Component, primaryKey, topic);
		}

		public void DeleteSubscription(ISubscriptionConfiguration configuration, string primaryKey, string topic)
		{
			Instance.SysProxy.Subscriptions.DeleteSubscription(configuration.Component, primaryKey, topic);
		}

		public void TriggerEvent<T>(ISubscriptionConfiguration configuration, string eventName, string primaryKey, string topic, T arguments)
		{
			Instance.SysProxy.Subscriptions.TriggerEvent(configuration.MicroService(), configuration.Component, eventName, primaryKey, topic, arguments is null ? null : Serializer.Serialize(arguments));
		}

		public List<IRecipient> QuerySubscribers(ISubscriptionConfiguration configuration, string primaryKey)
		{
			return Instance.SysProxy.Subscriptions.QuerySubscribers(configuration.Component, primaryKey).ToList();
		}

		public IRecipient SelectSubscriber(Guid token)
		{
			return Instance.SysProxy.Subscriptions.SelectSubscriber(token);
		}

		public IRecipient SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			return Instance.SysProxy.Subscriptions.SelectSubscriber(subscription, type, resourcePrimaryKey);
		}

		public Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags)
		{
			return Instance.SysProxy.Subscriptions.InsertSubscriber(subscription, type, resourcePrimaryKey, tags);
		}

		public void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			Instance.SysProxy.Subscriptions.DeleteSubscriber(subscription, type, resourcePrimaryKey);
		}

		public void InsertSubscribers(Guid subscription, List<IRecipient> recipients)
		{
			if (recipients is null || !recipients.Any())
				return;

			Instance.SysProxy.Subscriptions.InsertSubscribers(subscription, recipients);
		}

		public ISubscription SelectSubscription(Guid token)
		{
			return Instance.SysProxy.Subscriptions.SelectSubscription(token);
		}

		public ISubscription SelectSubscription(Guid configuration, string primaryKey)
		{
			return Instance.SysProxy.Subscriptions.SelectSubscription(configuration, primaryKey);
		}
	}
}
