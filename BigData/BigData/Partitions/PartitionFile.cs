using System;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFile : IPartitionFile
	{
		public DateTime StartTimestamp { get; set; }
		public DateTime EndTimestamp { get; set; }
		public int Count { get; set; }
		public PartitionFileStatus Status { get; set; }
		public Guid Node { get; set; }
		public Guid FileName { get; set; }
		public Guid Partition { get; set; }
		public string Key { get; set; }
		public Guid Timezone { get; set; }
	}
}
