using System;
using TomPIT.Data;

namespace TomPIT.BigData
{
	public interface IPartitionBuffer : IPrimaryKeyRecord
	{
		Guid Partition { get; }
		DateTime NextVisible { get; }
	}
}
