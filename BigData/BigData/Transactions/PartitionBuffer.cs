using System;

namespace TomPIT.BigData.Transactions
{
	internal class PartitionBuffer : IPartitionBuffer
	{
		public Guid Partition { get; set; }

		public DateTime NextVisible { get; set; }

		public long Count { get; set; }

		public DateTime LockTimeout { get; set; }

		public Guid UnlockKey { get; set; }
	}
}
