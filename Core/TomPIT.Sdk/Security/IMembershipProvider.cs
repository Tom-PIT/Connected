using System;
using System.Collections.Immutable;

namespace TomPIT.Security
{
	public interface IMembershipProvider
	{
		ImmutableList<IMembership> QueryMembership(Guid user);
		void Insert(Guid user, Guid role);
		void Delete(Guid user, Guid role);
	}
}
