using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class UserDataController : IUserDataController
	{
		public ImmutableList<IUserData> Query(Guid user)
		{
			return DataModel.UserData.Query(user).ToImmutableList();
		}

		public void Update(Guid user, List<IUserData> data)
		{
			DataModel.UserData.Update(user, data);
		}
	}
}
