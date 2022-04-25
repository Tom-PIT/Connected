using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class BigDataHandler : IBigDataHandler
	{
		private INodeHandler _nodes = null;
		private IPartitionHandler _partitions = null;
		private ITransactionHandler _transactions = null;
		private IPartitionBufferHandler _buffer = null;
		private ITimezoneHandler _timezones = null;

		public ITimezoneHandler Timezones => _timezones ??= new TimezoneHandler();

		public IPartitionBufferHandler Buffer
		{
			get
			{
				if (_buffer == null)
					_buffer = new PartitionBufferHandler();

				return _buffer;
			}
		}

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

		public ITransactionHandler Transactions
		{
			get
			{
				if (_transactions == null)
					_transactions = new TransactionHandler();

				return _transactions;
			}
		}
	}
}
