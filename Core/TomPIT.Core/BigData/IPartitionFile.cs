using System;

namespace TomPIT.BigData
{
	public enum PartitionFileStatus
	{
		Creating = 1,
		Open = 2,
		Closed = 3
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
