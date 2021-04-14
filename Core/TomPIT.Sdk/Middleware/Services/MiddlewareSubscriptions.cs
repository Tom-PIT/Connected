using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Security;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareSubscriptions : MiddlewareObject, IMiddlewareSubscriptions
	{
		public void Create(string subscription, string primaryKey)
		{
			Create(subscription, primaryKey, null);
		}

		public void Create(string subscription, string primaryKey, string topic)
		{
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			Context.Tenant.GetService<ISubscriptionService>().CreateSubscription(config.Configuration, primaryKey, topic);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey)
		{
			TriggerEvent(eventName, primaryKey, null);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, object arguments)
		{
			TriggerEvent(eventName, primaryKey, null, arguments);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic)
		{
			TriggerEvent(eventName, primaryKey, topic, null);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)]string eventName, string primaryKey, string topic, object arguments)
		{
			var config = ComponentDescriptor.Subscription(Context, eventName);

			config.Validate();

			var ev = config.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);

			if (ev == null)
				throw new RuntimeException($"{SR.ErrSubscriptionEventNotFound} ({config.MicroServiceName}/{config.ComponentName}/{config.Element})").WithMetrics(Context);

			Context.Tenant.GetService<ISubscriptionService>().TriggerEvent(config.Configuration, config.Element, primaryKey, topic, arguments);
		}

		public bool Exists([CIP(CIP.SubscriptionProvider)]string subscription, string primaryKey, string topic)
		{
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			return Context.Tenant.GetService<ISubscriptionService>().SubscriptionExists(config.Configuration, primaryKey, topic);
		}

		public List<IRecipient> QuerySubscribers(string subscription, string primaryKey)
		{
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			return Context.Tenant.GetService<ISubscriptionService>().QuerySubscribers(config.Configuration, primaryKey);
		}

		public Guid SubscribeUser(string subscription, string primaryKey, string identifier)
		{
			var user = Context.Tenant.GetService<IUserService>().Select(identifier);

			if (user == null)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			var sub = SelectSubscription(subscription, primaryKey);

			return Context.Tenant.GetService<ISubscriptionService>().InsertSubscriber(sub.Token, SubscriptionResourceType.User, user.Token.ToString());
		}

		public Guid SubscribeRole(string subscription, string primaryKey, string roleName)
		{
			var role = Context.Tenant.GetService<IRoleService>().Select(roleName);

			if (role == null)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			var sub = SelectSubscription(subscription, primaryKey);

			return Context.Tenant.GetService<ISubscriptionService>().InsertSubscriber(sub.Token, SubscriptionResourceType.Role, role.Token.ToString());
		}

		public IRecipient SelectSubscriber(string subscription, string primaryKey, SubscriptionResourceType type, string identifier)
		{
			var sub = SelectSubscription(subscription, primaryKey);

			return Context.Tenant.GetService<ISubscriptionService>().SelectSubscriber(sub.Token, type, identifier);
		}

		public IRecipient SelectSubscriber(Guid token)
		{
			return Context.Tenant.GetService<ISubscriptionService>().SelectSubscriber(token);
		}

		public Guid SubscribeAlien(string subscription, string primaryKey, string email)
		{
			var sub = SelectSubscription(subscription, primaryKey);

			var alien = Context.Tenant.GetService<IAlienService>().Select(email);
			var alienToken = Guid.Empty;

			if (alien == null)
				alienToken = Context.Tenant.GetService<IAlienService>().Insert(null, null, email, null, null, Guid.Empty, null);
			else
				alienToken = alien.Token;

			return Context.Tenant.GetService<ISubscriptionService>().InsertSubscriber(sub.Token, SubscriptionResourceType.Alien, alienToken.ToString());
		}

		public void UnsubscribeUser(string subscription, string primaryKey, string identifier)
		{
			var user = Context.Tenant.GetService<IUserService>().Select(identifier);

			if (user == null)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			var sub = SelectSubscription(subscription, primaryKey);

			Context.Tenant.GetService<ISubscriptionService>().DeleteSubscriber(sub.Token, SubscriptionResourceType.User, user.Token.ToString());
		}

		public void UnsubscribeRole(string subscription, string primaryKey, string roleName)
		{
			var role = Context.Tenant.GetService<IRoleService>().Select(roleName);

			if (role == null)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			var sub = SelectSubscription(subscription, primaryKey);

			Context.Tenant.GetService<ISubscriptionService>().DeleteSubscriber(sub.Token, SubscriptionResourceType.Role, role.Token.ToString());
		}

		public void UnsubscribeAlien(string subscription, string primaryKey, string email)
		{
			var sub = SelectSubscription(subscription, primaryKey);

			Context.Tenant.GetService<ISubscriptionService>().DeleteSubscriber(sub.Token, SubscriptionResourceType.Alien, email);
		}

		private ISubscription SelectSubscription(string subscription, string primaryKey)
		{
			var descriptor = ComponentDescriptor.Subscription(Context, subscription);

			descriptor.Validate();

			var sub = Context.Tenant.GetService<ISubscriptionService>().SelectSubscription(descriptor.Component.Token, primaryKey);

			if (sub == null)
				throw new RuntimeException(SR.ErrSubscriptionNotFound);

			return sub;
		}
	}
}
