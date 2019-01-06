using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Security
{
	internal class RoleAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Roles";

		public AuthorizationProviderResult Authorize(IExecutionContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			var roles = state["roles"] as List<Guid>;

			if (roles.Contains(permission.Evidence))
			{
				switch (permission.Value)
				{
					case PermissionValue.NotSet:
						return AuthorizationProviderResult.NotHandled;
					case PermissionValue.Allow:
						return AuthorizationProviderResult.Success;
					case PermissionValue.Deny:
						return AuthorizationProviderResult.Fail;
					default:
						throw new NotSupportedException();
				}
			}

			return AuthorizationProviderResult.NotHandled;
		}

		public AuthorizationProviderResult PreAuthorize(IExecutionContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			var roles = ResolveImplicitRoles(context, e);

			if (e.User != Guid.Empty)
			{
				var membership = context.Connection().GetService<IAuthorizationService>() as IMembershipProvider;

				if (membership != null)
				{
					var list = membership.QueryMembership(e.User);

					if (list.Count > 0)
						roles.AddRange(list.Select(f => f.Role));
				}
			}

			if (roles.Contains(SecurityUtils.FullControlRole))
				return AuthorizationProviderResult.Success;

			state.Add("roles", roles);

			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IExecutionContext context)
		{
			var roles = context.Connection().GetService<IRoleService>().Query();
			var r = new List<IPermissionSchemaDescriptor>();

			foreach (var i in roles)
			{
				r.Add(new SchemaDescriptor
				{
					Title = i.Name,
					Id = i.Token
				});
			}

			return r;
		}

		private List<Guid> ResolveImplicitRoles(IExecutionContext context, AuthorizationArgs e)
		{
			var r = new List<Guid>
			{
				SecurityUtils.EveryoneRole
			};

			if (e.User == Guid.Empty)
				r.Add(SecurityUtils.AnonymousRole);
			else
			{
				var u = context.Connection().GetService<IUserService>().Select(e.User.ToString());

				if (u == null)
				{
					context.Connection().LogWarning(null, GetType().ShortName(), "Authenticated user not found. Request will be treated as anonymous.");
					return r;
				}

				r.Add(SecurityUtils.AuthenticatedRole);

				if (u.LoginName.Contains('\\'))
					r.Add(SecurityUtils.DomainIdentityRole);
				else
					r.Add(SecurityUtils.LocalIdentityRole);
			}

			return r;
		}

		public static bool IsInImplicitRole(ISysConnection connection, Guid user, IRole role)
		{
			if (role.Token == SecurityUtils.AnonymousRole)
				return user == Guid.Empty;
			else if (role.Token == SecurityUtils.AuthenticatedRole)
				return user != Guid.Empty;
			else if (role.Token == SecurityUtils.EveryoneRole)
				return true;
			else
			{
				var u = connection.GetService<IUserService>().Select(user.ToString());

				if (u == null)
					return false;

				if (role.Token == SecurityUtils.LocalIdentityRole)
					return u.IsLocal();
				else if (role.Token == SecurityUtils.DomainIdentityRole)
					return !u.IsLocal();
			}

			return false;
		}
	}
}