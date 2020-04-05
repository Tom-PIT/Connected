using System;
using System.Collections.Generic;
using TomPIT.Cdn;

using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareSubscriptions
	{
		bool Exists([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void Create([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey);
		void Create([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string topic);
		void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey);
		void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, object arguments);
		void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic);
		void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, object arguments);
		List<IRecipient> QuerySubscribers([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey);
		IRecipient SelectSubscriber([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, SubscriptionResourceType type, string resourcePrimaryKey);
		IRecipient SelectSubscriber(Guid token);
		Guid SubscribeUser([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string identifier);
		Guid SubscribeRole([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string roleName);
		Guid SubscribeAlien([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string email);
		void UnsubscribeUser([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string identifier);
		void UnsubscribeRole([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string roleName);
		void UnsubscribeAlien([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string email);
	}
}
