using System;

namespace TomPIT.Security
{
	public class MembershipEventArgs : EventArgs
	{
		public MembershipEventArgs()
		{

		}
		public MembershipEventArgs(Guid user, Guid role)
		{
			User = user;
			Role = role;
		}

		public Guid User { get; set; }
		public Guid Role { get; set; }
	}
}
