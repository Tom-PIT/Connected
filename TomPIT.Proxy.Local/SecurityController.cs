using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;
using TomPIT.Sys.Model;
using TomPIT.Sys.Security;

namespace TomPIT.Proxy.Local
{
	internal class SecurityController : ISecurityController
	{
		public ImmutableList<IAuthenticationToken> QueryAuthenticationTokens()
		{
			return DataModel.AuthenticationTokens.Query();
		}

		public ImmutableList<IAuthenticationToken> QueryAuthenticationTokens(List<string> resourceGroups)
		{
			return DataModel.AuthenticationTokens.Query(resourceGroups);
		}

		public ImmutableList<IMembership> QueryMembership()
		{
			return DataModel.Membership.Query();
		}

		public ImmutableList<IPermission> QueryPermissions()
		{
			return DataModel.Permissions.Query();
		}

		public ImmutableList<IPermission> QueryPermissions(List<string> resourceGroups)
		{
			return DataModel.Permissions.Query(resourceGroups);
		}

		public IAuthenticationToken SelectAuthenticationToken(Guid token)
		{
			return DataModel.AuthenticationTokens.Select(token);
		}

		public IPermission SelectPermission(string evidence, string schema, string claim, string primaryKey, string descriptor)
		{
			return DataModel.Permissions.Select(evidence, schema, claim, primaryKey, descriptor);
		}

		public ImmutableList<IPermission> SelectPermissions(string primaryKey)
		{
			return DataModel.Permissions.Query(primaryKey);
		}

		public IValidationParameters SelectValidationParameters()
		{
			return new ValidationParameters
			{
				IssuerSigningKey = TomPITAuthenticationHandler.IssuerSigningKey,
				ValidAudience = TomPITAuthenticationHandler.ValidAudience,
				ValidIssuer = TomPITAuthenticationHandler.ValidIssuer
			};
		}
	}
}
