using System;

namespace TomPIT.Security
{
	public class UserEventArgs : EventArgs
	{
		public UserEventArgs()
		{

		}
		public UserEventArgs(Guid user)
		{
			User = user;
		}

		public Guid User { get; set; }
	}
}
