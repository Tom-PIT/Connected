using System;
using TomPIT.Data;

namespace TomPIT.Proxy.Remote
{
	internal class Lock : ILock
	{
		public string Entity { get; set; }

		public Guid UnlockKey { get; set; }

		public DateTime Timeout { get; set; }
	}
}
