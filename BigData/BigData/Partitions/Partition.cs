using System;

namespace TomPIT.BigData.Partitions
{
	internal class Partition : IPartition
	{
		public Guid Configuration { get; set; }

		public int FileCount { get; set; }

		public PartitionStatus Status { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public Guid ResourceGroup { get; set; }
	}
}
