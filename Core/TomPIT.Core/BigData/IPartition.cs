using System;

namespace TomPIT.BigData
{
	public enum PartitionStatus
	{
		Active = 1,
		Maintenance = 2
	}

	public interface IPartition
	{
		Guid Configuration { get; }
		int FileCount { get; }
		PartitionStatus Status { get; }
		string Name { get; }
		DateTime Created { get; }
	}
}
