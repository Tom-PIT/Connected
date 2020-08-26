using System;

namespace TomPIT.BigData
{
	public interface IPartitionBuffer
	{
		Guid Partition { get; }
		DateTime NextVisible { get; }
	}
}
