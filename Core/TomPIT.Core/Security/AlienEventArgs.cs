using System;

namespace TomPIT.Security
{
	public class AlienEventArgs : EventArgs
	{
		public AlienEventArgs(Guid alien)
		{
			Alien = alien;
		}

		public Guid Alien { get; }
	}
}
