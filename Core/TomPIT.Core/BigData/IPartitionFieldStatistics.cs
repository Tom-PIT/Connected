using System;

namespace TomPIT.BigData
{
	public interface IPartitionFieldStatistics
	{
		Guid Partition { get; }
		Guid File { get; }
		string Key { get; }
		string StartString { get; }
		string EndString { get; }
		decimal StartNumber { get; }
		decimal EndNumber { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		string FieldName { get; }
	}
}
