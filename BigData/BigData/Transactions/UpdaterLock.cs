using System;

namespace TomPIT.BigData.Transactions
{
	internal class UpdaterLock
	{
		public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddSeconds(60);

		public bool Expired => Timestamp == DateTime.MinValue || DateTime.UtcNow > Timestamp;

		public void Lock()
		{
			Timestamp = DateTime.UtcNow.AddSeconds(60);
		}

		public void Release()
		{
			Timestamp = DateTime.MinValue;
		}
	}
}
