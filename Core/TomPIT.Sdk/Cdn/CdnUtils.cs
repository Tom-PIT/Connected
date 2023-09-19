using System;
using System.Collections.Generic;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Cdn
{
	public static class CdnUtils
	{
		public static IRecipient CreateUserRecipient(IMiddlewareContext context, string identifier)
		{
			return CreateUserRecipient(context, identifier, null);
		}
		public static IRecipient CreateUserRecipient(IMiddlewareContext context, string identifier, List<string> tags)
		{
			var user = context.Tenant.GetService<IUserService>().Select(identifier);

			if (user == null)
				throw new RuntimeException($"{SR.ErrUserNotFound} ({identifier})");

			return new Recipient
			{
				Type = SubscriptionResourceType.User,
				ResourcePrimaryKey = user.Token.ToString(),
				Tags = tags
			};
		}

		public static IRecipient CreateRoleRecipient(IMiddlewareContext context, string roleName, List<string> tags)
		{
			var role = context.Tenant.GetService<IRoleService>().Select(roleName);

			if (role == null)
				throw new RuntimeException($"{SR.ErrRoleNotFound} ({roleName})");

			return new Recipient
			{
				Type = SubscriptionResourceType.Role,
				ResourcePrimaryKey = role.Token.ToString(),
				Tags = tags
			};
		}
		public static IRecipient CreateRoleRecipient(IMiddlewareContext context, string roleName)
		{
			return CreateRoleRecipient(context, roleName, null);
		}

		public static IRecipient CreateAlienRecipient(IMiddlewareContext context, string email)
		{
			return CreateAlienRecipient(context, null, null, email, null, null, Guid.Empty, null, null, null, null);
		}
		public static IRecipient CreateAlienRecipient(IMiddlewareContext context, string firstName = null, string lastName = null, string email = null, string mobile = null, string phone = null,
			Guid language = default, string timezone = null, string resourceType = null, string resourcePrimaryKey = null, List<string> tags = null)
		{
			var service = context.Tenant.GetService<IAlienService>();
			
			if(service.Select(firstName, lastName, email, mobile, phone, resourceType, resourcePrimaryKey) is not IAlien alien)
				alien = service.Select(service.Insert(firstName, lastName, email, mobile, phone, language, timezone, resourceType, resourcePrimaryKey));

			return new Recipient
			{
				Type = SubscriptionResourceType.Alien,
				ResourcePrimaryKey = alien.Token.ToString(),
				Tags = tags
			};
		}
	}
}
