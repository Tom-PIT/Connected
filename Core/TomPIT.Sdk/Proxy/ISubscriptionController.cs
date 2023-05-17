using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;

namespace TomPIT.Proxy
{
	public interface ISubscriptionController
	{
		bool SubscriptionExists(Guid configuration, string primaryKey, string topic);
		void CreateSubscription(Guid microService, Guid configuration, string primaryKey, string topic);
		void DeleteSubscription(Guid configuration, string primaryKey, string topic);
		void TriggerEvent(Guid microService, Guid configuration, string name, string primaryKey, string topic, string arguments);

		ISubscription SelectSubscription(Guid token);
		ISubscription SelectSubscription(Guid configuration, string primaryKey);
		ImmutableList<IRecipient> QuerySubscribers(Guid configuration, string primaryKey);
		IRecipient SelectSubscriber(Guid token);
		IRecipient SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey);
		Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags = null);
		void InsertSubscribers(Guid subscription, List<IRecipient> recipients);
		void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey);
	}
}
