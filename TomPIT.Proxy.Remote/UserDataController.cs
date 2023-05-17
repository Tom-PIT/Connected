using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class UserDataController : IUserDataController
	{
		private const string Controller = "UserData";
		public ImmutableList<IUserData> Query(Guid user)
		{
			return Connection.Post<List<UserData>>(Connection.CreateUrl(Controller, "Query"), new
			{
				user
			}).ToImmutableList<IUserData>();
		}

		public void Update(Guid user, List<IUserData> items)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				user,
				items
			});
		}
	}
}
