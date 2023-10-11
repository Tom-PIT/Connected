using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
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

		public void Delete(string subscription, string primaryKey, string topic)
		{
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			Context.Tenant.GetService<ISubscriptionService>().DeleteSubscription(config.Configuration, primaryKey, topic);
		}

		public void Create(string subscription, string primaryKey, string topic)
		{
			var config = ComponentDescriptor.Subscription(Context, subscription);

			config.Validate();

			Context.Tenant.GetService<ISubscriptionService>().CreateSubscription(config.Configuration, primaryKey, topic);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)] string eventName, string primaryKey)
		{
			TriggerEvent(eventName, primaryKey, null);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)] string eventName, string primaryKey, object arguments)
		{
			TriggerEvent(eventName, primaryKey, null, arguments);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)] string eventName, string primaryKey, string topic)
		{
			TriggerEvent(eventName, primaryKey, topic, null);
		}

		public void TriggerEvent([CIP(CIP.SubscriptionEventProvider)] string eventName, string primaryKey, string topic, object arguments)
		{
			var config = ComponentDescriptor.Subscription(Context, eventName);

			config.Validate();

			var ev = config.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, config.Element, true) == 0);

			if (ev == null)
				throw new RuntimeException($"{SR.ErrSubscriptionEventNotFound} ({config.MicroServiceName}/{config.ComponentName}/{config.Element})");

			Context.Tenant.GetService<ISubscriptionService>().TriggerEvent(config.Configuration, config.Element, primaryKey, topic, arguments);
		}

		public bool Exists([CIP(CIP.SubscriptionProvider)] string subscription, string primaryKey, string topic)
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

		public Guid SubscribeUser(string subscription, string primaryKey, string identifier, List<string> tags)
		{
			if (Context.Tenant.GetService<IUserService>().Select(identifier) is not IUser user)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			var sub = SelectSubscription(subscription, primaryKey);

			return Context.Tenant.GetService<ISubscriptionService>().InsertSubscriber(sub.Token, SubscriptionResourceType.User, user.Token.ToString(), tags);

		}
		public Guid SubscribeUser(string subscription, string primaryKey, string identifier)
		{
			return SubscribeUser(subscription, primaryKey, identifier, null);
		}

		public Guid SubscribeRole(string subscription, string primaryKey, string roleName)
		{
			return SubscribeRole(subscription, primaryKey, roleName, null);
		}
		public Guid SubscribeRole(string subscription, string primaryKey, string roleName, List<string> tags)
		{
			if (Context.Tenant.GetService<IRoleService>().Select(roleName) is not IRole role)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			var sub = SelectSubscription(subscription, primaryKey);

			return Context.Tenant.GetService<ISubscriptionService>().InsertSubscriber(sub.Token, SubscriptionResourceType.Role, role.Token.ToString(), tags);
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
			return SubscribeAlien(subscription, primaryKey, email, null);
		}
		public Guid SubscribeAlien(string subscription, string primaryKey, string email, List<string> tags)
		{
			return SubscribeAlien(subscription, primaryKey, null, null, email, null, null, Guid.Empty, null, null, null, tags);
		}

		public Guid SubscribeAlien(string subscription, string primaryKey, string firstName = null, string lastName = null, string email = null, string mobile = null, string phone = null, Guid language = default, string timezone = null, string resourceType = null, string resourcePrimaryKey = null, List<string> tags = null)
		{
			var sub = SelectSubscription(subscription, primaryKey);
			var alien = Context.Tenant.GetService<IAlienService>().Select(firstName, lastName, email, mobile, phone, resourceType, resourcePrimaryKey);
			Guid alienToken;

			if (alien is null)
				alienToken = Context.Tenant.GetService<IAlienService>().Insert(firstName, lastName, email, phone, mobile, language, timezone, resourceType, resourcePrimaryKey);
			else
				alienToken = alien.Token;

			return Context.Tenant.GetService<ISubscriptionService>().InsertSubscriber(sub.Token, SubscriptionResourceType.Alien, alienToken.ToString(), tags);
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
			UnsubscribeAlien(subscription, primaryKey, null, null, email, null, null, null, null);
		}

		public void UnsubscribeAlien(string subscription, string primaryKey, string firstName = null, string lastName = null, string email = null, string mobile = null, string phone = null, string resourceType = null, string resourcePrimaryKey = null)
		{
			var sub = SelectSubscription(subscription, primaryKey);

			if (Context.Tenant.GetService<IAlienService>().Select(firstName, lastName, email, mobile, phone, resourceType, resourcePrimaryKey) is IAlien alien)
				Context.Tenant.GetService<ISubscriptionService>().DeleteSubscriber(sub.Token, SubscriptionResourceType.Alien, alien.Token.ToString());
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
