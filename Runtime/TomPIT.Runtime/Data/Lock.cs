using System;

namespace TomPIT.Data
{
	internal class Lock : ILock
	{
		public string Entity { get; set; }

		public Guid UnlockKey { get; set; }

		public DateTime Timeout { get; set; }
	}
}
