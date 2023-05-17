using System;
using System.Collections.Immutable;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class RoleController : IRoleController
	{
		public ImmutableList<IRole> Query()
		{
			return DataModel.Roles.Query();
		}

		public IRole Select(Guid token)
		{
			return DataModel.Roles.Select(token);
		}
	}
}
