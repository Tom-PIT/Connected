using System;

namespace TomPIT.Security
{
	public class MembershipEventArgs : EventArgs
	{
		public MembershipEventArgs(Guid user, Guid role)
		{
			User = user;
			Role = role;
		}

		public Guid User { get; }
		public Guid Role { get; }
	}
}
