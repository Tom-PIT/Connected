using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class SubscriptionController : ISubscriptionController
	{
		public void CreateSubscription(Guid microService, Guid configuration, string primaryKey, string topic)
		{
			DataModel.Subscriptions.Enqueue(microService, configuration, topic, primaryKey);
		}

		public void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			DataModel.Subscriptions.DeleteSubscriber(subscription, type, resourcePrimaryKey);
		}

		public void DeleteSubscription(Guid configuration, string primaryKey, string topic)
		{
			DataModel.Subscriptions.Delete(configuration, topic, primaryKey);
		}

		public Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags = null)
		{
			return DataModel.Subscriptions.InsertSubscriber(subscription, type, resourcePrimaryKey, tags);
		}

		public void InsertSubscribers(Guid subscription, List<IRecipient> recipients)
		{
			DataModel.Subscriptions.InsertSubscribers(subscription, recipients);
		}

		public ImmutableList<IRecipient> QuerySubscribers(Guid configuration, string primaryKey)
		{
			return DataModel.Subscriptions.QuerySubscribers(configuration, null, primaryKey).ToImmutableList<IRecipient>();
		}

		public IRecipient SelectSubscriber(Guid token)
		{
			return DataModel.Subscriptions.SelectSubscriber(token);
		}

		public IRecipient SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			return DataModel.Subscriptions.SelectSubscriber(subscription, type, resourcePrimaryKey);
		}

		public ISubscription SelectSubscription(Guid token)
		{
			return DataModel.Subscriptions.Select(token);
		}

		public ISubscription SelectSubscription(Guid configuration, string primaryKey)
		{
			return DataModel.Subscriptions.Select(configuration, null, primaryKey);
		}

		public bool SubscriptionExists(Guid configuration, string primaryKey, string topic)
		{
			return DataModel.Subscriptions.Select(configuration, topic, primaryKey) is not null;
		}

		public void TriggerEvent(Guid microService, Guid configuration, string name, string primaryKey, string topic, string arguments)
		{
			DataModel.Subscriptions.InsertSubscriptionEvent(microService, configuration, name, topic, primaryKey, arguments);
		}
	}
}
