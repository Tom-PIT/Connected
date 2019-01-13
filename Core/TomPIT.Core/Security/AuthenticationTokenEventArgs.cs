using System;

namespace TomPIT.Security
{
	public class AuthenticationTokenEventArgs : EventArgs
	{
		public AuthenticationTokenEventArgs(Guid token)
		{
			Token = token;
		}

		public Guid Token { get; }
	}
}
