using System;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy
{
	public interface IRoleController
	{
		ImmutableList<IRole> Query();
		IRole Select(Guid token);
	}
}
