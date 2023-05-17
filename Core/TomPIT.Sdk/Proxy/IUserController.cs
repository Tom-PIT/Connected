using System;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy
{
	public interface IUserController
	{
		IMembership SelectMembership(Guid user, Guid role);
		ImmutableList<IUser> Query();
		IUser Select(string qualifier);
		IUser SelectByAuthenticationToken(Guid token);
		IUser SelectBySecurityCode(string securityCode);
	}
}
