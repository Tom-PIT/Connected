using System;
using System.Collections.Generic;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Cdn
{
	public abstract class SubscriptionMiddleware : MiddlewareComponent, ISubscriptionMiddleware
	{
		protected ISubscription Subscription { get; private set; }
		public void Created(ISubscription subscription)
		{
			Subscription = subscription;

			OnCreated();
		}


		protected virtual void OnCreated()
		{

		}

		public List<IRecipient> Invoke(ISubscription subscription)
		{
			Validate();

			Subscription = subscription;

			return OnInvoke();
		}

		protected virtual List<IRecipient> OnInvoke()
		{
			return null;
		}

		public IRecipient CreateUserRecipient(string identifier)
		{
			var user = Context.Tenant.GetService<IUserService>().Select(identifier);

			if (user == null)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			return new Recipient
			{
				Type = SubscriptionResourceType.User,
				ResourcePrimaryKey = user.Token.ToString()
			};
		}

		public IRecipient CreateRoleRecipient(string roleName)
		{
			var role = Context.Tenant.GetService<IRoleService>().Select(roleName);

			if (role == null)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			return new Recipient
			{
				Type = SubscriptionResourceType.Role,
				ResourcePrimaryKey = role.Token.ToString()
			};
		}

		public IRecipient CreateAlienRecipient(string email)
		{
			var service = Context.Tenant.GetService<IAlienService>();
			var alien = service.Select(email);

			if (alien == null)
				alien = service.Select(service.Insert(null, null, email, null, null, Guid.Empty, null));

			return new Recipient
			{
				Type = SubscriptionResourceType.Alien,
				ResourcePrimaryKey = alien.Token.ToString()
			};
		}
	}
}
