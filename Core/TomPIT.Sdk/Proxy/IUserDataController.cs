using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy
{
	public interface IUserDataController
	{
		ImmutableList<IUserData> Query(Guid user);
		void Update(Guid user, List<IUserData> data);
	}
}
