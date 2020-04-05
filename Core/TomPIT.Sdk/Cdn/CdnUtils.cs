using System;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Cdn
{
	public static class CdnUtils
	{
		public static IRecipient CreateUserRecipient(IMiddlewareContext context, string identifier)
		{
			var user = context.Tenant.GetService<IUserService>().Select(identifier);

			if (user == null)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			return new Recipient
			{
				Type = SubscriptionResourceType.User,
				ResourcePrimaryKey = user.Token.ToString()
			};
		}

		public static IRecipient CreateRoleRecipient(IMiddlewareContext context, string roleName)
		{
			var role = context.Tenant.GetService<IRoleService>().Select(roleName);

			if (role == null)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			return new Recipient
			{
				Type = SubscriptionResourceType.Role,
				ResourcePrimaryKey = role.Token.ToString()
			};
		}

		public static IRecipient CreateAlienRecipient(IMiddlewareContext context, string email)
		{
			var service = context.Tenant.GetService<IAlienService>();
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
