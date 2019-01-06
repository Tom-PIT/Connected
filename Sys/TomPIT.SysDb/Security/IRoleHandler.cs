using System;
using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
	public interface IRoleHandler
	{
		List<IRole> Query();
		IRole Select(Guid token);

		void Insert(Guid token, string name);
		void Update(IRole role, string name);
		void Delete(IRole role);
	}
}
