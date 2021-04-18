using System;

namespace TomPIT.Environment
{
	public class ClientEventArgs : EventArgs
	{
		public ClientEventArgs()
		{

		}
		public ClientEventArgs(string token)
		{
			Token = token;
		}

		public string Token { get; set; }
	}
}
