using System;
using TomPIT.BigData;
using TomPIT.Data;

namespace TomPIT.Sys.Model.BigData
{
	internal class PartitionBuffer : PrimaryKeyRecord, IPartitionBuffer
	{
		public PartitionBuffer()
		{

		}

		public PartitionBuffer(IPartitionBuffer item) : base(item)
		{
			Partition = item.Partition;
			NextVisible = item.NextVisible;
		}
		public Guid Partition { get; set; }

		public DateTime NextVisible { get; set; }
	}
}
