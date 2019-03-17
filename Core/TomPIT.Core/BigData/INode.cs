using System;

namespace TomPIT.BigData
{
	public enum NodeStatus
	{
		Active = 1,
		Inactive = 2
	}

	public interface INode
	{
		string Name { get; }
		string ConnectionString { get; }
		string AdminConnectionString { get; }
		Guid Token { get; }
		NodeStatus Status { get; }
		long Size { get; }
	}
}
