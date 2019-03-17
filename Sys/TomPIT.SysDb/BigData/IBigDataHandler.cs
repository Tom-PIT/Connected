namespace TomPIT.SysDb.BigData
{
	public interface IBigDataHandler
	{
		INodeHandler Nodes { get; }
		IPartitionHandler Partitions { get; }
	}
}
