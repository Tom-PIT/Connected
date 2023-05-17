using System;
using System.Collections.Immutable;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class UserController : IUserController
	{
		public ImmutableList<IUser> Query()
		{
			return DataModel.Users.Query();
		}

		public IUser Select(string qualifier)
		{
			return DataModel.Users.Resolve(qualifier);
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			return DataModel.Users.SelectByAuthenticationToken(token);
		}

		public IUser SelectBySecurityCode(string securityCode)
		{
			return DataModel.Users.SelectBySecurityCode(securityCode);
		}

		public IMembership SelectMembership(Guid user, Guid role)
		{
			return DataModel.Membership.Select(user, role);
		}
	}
}
