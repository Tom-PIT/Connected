using System;
using System.Collections.Generic;
using TomPIT.ComponentModel.Cdn;

namespace TomPIT.Cdn
{
	public interface ISubscriptionService
	{
		bool SubscriptionExists(ISubscriptionConfiguration configuration, string primaryKey, string topic);
		void CreateSubscription(ISubscriptionConfiguration configuration, string primaryKey, string topic);
		void DeleteSubscription(ISubscriptionConfiguration configuration, string primaryKey, string topic);
		void TriggerEvent<T>(ISubscriptionConfiguration configuration, string name, string primaryKey, string topic, T arguments);

		ISubscription SelectSubscription(Guid token);
		ISubscription SelectSubscription(Guid configuration, string primaryKey);
		List<IRecipient> QuerySubscribers(ISubscriptionConfiguration configuration, string primaryKey);
		IRecipient SelectSubscriber(Guid token);
		IRecipient SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey);
		Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey, List<string> tags = null);
		void InsertSubscribers(Guid subscription, List<IRecipient> recipients);
		void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey);

	}
}
