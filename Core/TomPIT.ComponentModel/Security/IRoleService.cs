using System;
using System.Collections.Generic;

namespace TomPIT.Security
{
	public interface IRoleService
	{
		List<IRole> Query();
		IRole Select(Guid token);
		IRole Select(string name);
	}
}
