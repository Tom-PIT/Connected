using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class UserController : IUserController
	{
		private const string Controller = "User";

		public ImmutableList<IUser> Query()
		{
			return Connection.Get<List<User>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IUser>();
		}

		public IUser Select(string qualifier)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("qualifier", qualifier);

			return Connection.Get<User>(u);
		}

		public IUser SelectByAuthenticationToken(Guid token)
		{
			var u = Connection.CreateUrl(Controller, "SelectByAuthenticationToken")
				.AddParameter("token", token);

			return Connection.Get<User>(u);
		}

		public IUser SelectBySecurityCode(string securityCode)
		{
			return Connection.Post<User>(Connection.CreateUrl(Controller, "SelectBySecurityCode"), new
			{
				securityCode
			});
		}

		public IMembership SelectMembership(Guid user, Guid role)
		{
			var u = Connection.CreateUrl(Controller, "SelectMembership")
				.AddParameter("user", user)
				.AddParameter("role", role);

			return Connection.Get<Membership>(u);
		}
	}
}
