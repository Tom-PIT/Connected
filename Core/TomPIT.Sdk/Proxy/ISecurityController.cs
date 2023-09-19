using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy
{
	public interface ISecurityController
	{
		ImmutableList<IPermission> QueryPermissions();
		ImmutableList<IPermission> QueryPermissions(List<string> resourceGroups);
		ImmutableList<IPermission> SelectPermissions(string primaryKey);
		IPermission SelectPermission(string evidence, string schema, string claim, string primaryKey, string descriptor);
		ImmutableList<IMembership> QueryMembership();
		IValidationParameters SelectValidationParameters();
		ImmutableList<IAuthenticationToken> QueryAuthenticationTokens();
		ImmutableList<IAuthenticationToken> QueryAuthenticationTokens(List<string> resourceGroups);
		IAuthenticationToken SelectAuthenticationToken(Guid token);
	}
}
