using System;
using System.Collections.Generic;

namespace TomPIT.Security
{
	public interface IMembershipProvider
	{
		List<IMembership> QueryMembership(Guid user);
	}
}
