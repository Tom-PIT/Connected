using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class BigDataHandler : IBigDataHandler
	{
		private INodeHandler _nodes = null;
		private IPartitionHandler _partitions = null;

		public INodeHandler Nodes
		{
			get
			{
				if (_nodes == null)
					_nodes = new NodeHandler();

				return _nodes;
			}
		}

		public IPartitionHandler Partitions
		{
			get
			{
				if (_partitions == null)
					_partitions = new PartitionHandler();

				return _partitions;
			}
		}
	}
}
