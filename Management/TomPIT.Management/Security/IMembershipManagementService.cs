using System;
using System.Collections.Generic;

namespace TomPIT.Security
{
	public interface IMembershipManagementService
	{
		List<IMembership> Query(Guid role);
		void Insert(Guid user, Guid role);
		void Delete(Guid user, Guid role);
	}
}
