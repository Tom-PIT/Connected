using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Threading
{
	public class Locker
	{
		public Locker()
		{
			Sync = new object();
		}

		public object Sync { get; }
	}
}
