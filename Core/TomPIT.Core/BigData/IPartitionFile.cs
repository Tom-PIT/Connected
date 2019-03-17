using System;

namespace TomPIT.BigData
{
	public enum PartitionFileStatus
	{
		Open = 1,
		Closed = 2
	}

	public interface IPartitionFile
	{
		DateTime StartTimestamp { get; }
		DateTime EndTimestamp { get; }
		int Count { get; }
		PartitionFileStatus Status { get; }
		Guid Node { get; }
		Guid FileName { get; }
		Guid Partition { get; }
		string Key { get; }
	}
}
