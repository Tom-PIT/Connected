using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class RoleController : IRoleController
	{
		private const string Controller = "Role";
		public ImmutableList<IRole> Query()
		{
			return Connection.Get<List<Role>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IRole>();
		}

		public IRole Select(Guid token)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("token", token);

			return Connection.Get<Role>(u);
		}
	}
}
