using System;

namespace TomPIT.Security
{
	public class RoleEventArgs : EventArgs
	{
		public RoleEventArgs()
		{

		}
		public RoleEventArgs(Guid role)
		{
			Role = role;
		}

		public Guid Role { get; set; }
	}
}
