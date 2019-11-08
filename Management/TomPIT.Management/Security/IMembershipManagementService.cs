using System;
using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	public interface IMembershipManagementService
	{
		List<IMembership> Query(Guid role);
		void Insert(Guid user, Guid role);
		void Delete(Guid user, Guid role);
	}
}
