using System;
using System.Threading;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class Transaction : LongPrimaryKeyRecord, IServerTransaction
	{
		private int _blockRemaining = 0;
		public int BlockCount { get; set; }
		public int BlockRemaining => _blockRemaining;
		public Guid Partition { get; set; }
		public DateTime Created { get; set; }
		public Guid Token { get; set; }
		public TransactionStatus Status { get; set; }

		public void DecrementBlock()
		{
			Interlocked.Decrement(ref _blockRemaining);
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			BlockCount = GetInt("block_count");
			_blockRemaining = GetInt("block_remaining");
			Partition = GetGuid("partition_token");
			Created = GetDate("created");
			Token = GetGuid("token");
			Status = GetValue("status", TransactionStatus.Pending);
		}
	}
}
