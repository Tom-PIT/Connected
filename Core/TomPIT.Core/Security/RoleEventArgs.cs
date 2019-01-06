using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Security
{
	public class RoleEventArgs : EventArgs
	{
		public RoleEventArgs(Guid role)
		{
			Role = role;
		}

		public Guid Role { get; }
	}
}
