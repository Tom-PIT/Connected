using System;
using System.Collections.Immutable;

namespace TomPIT.Security
{
	public interface IMembershipProvider
	{
		ImmutableList<IMembership> QueryMembership(Guid user);
	}
}
