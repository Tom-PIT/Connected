using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Data;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Cdn
{
	public class SubscriptionSubscribeArguments : DataModelContext
	{
		private List<IRecipient> _subscribers = null;

		public SubscriptionSubscribeArguments(IExecutionContext sender, TomPIT.Cdn.ISubscription subscription) : base(sender)
		{
			Subscription = subscription;
		}

		public TomPIT.Cdn.ISubscription Subscription { get; }

		public void AddUser(string identifier)
		{
			var user = Connection.GetService<IUserService>().Select(identifier);

			if (user == null)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			Subscribers.Add(new Recipient
			{
				Type = SubscriptionResourceType.User,
				ResourcePrimaryKey = user.Token.ToString()
			});
		}

		public void AddRole(string roleName)
		{
			var role = Connection.GetService<IRoleService>().Select(roleName);

			if (role == null)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			Subscribers.Add(new Recipient
			{
				Type = SubscriptionResourceType.Role,
				ResourcePrimaryKey = role.Token.ToString()
			});
		}

		public void AddAlien(string email)
		{
			var service = Connection.GetService<IAlienService>();
			var alien = service.Select(email);

			if (alien == null)
				alien = service.Select(service.Insert(null, null, email, null, null, Guid.Empty, null));

			Subscribers.Add(new Recipient
			{
				Type = SubscriptionResourceType.Alien,
				ResourcePrimaryKey = alien.Token.ToString()
			});
		}

		public List<IRecipient> Subscribers
		{
			get
			{
				if (_subscribers == null)
					_subscribers = new List<IRecipient>();

				return _subscribers;
			}
		}
	}
}
