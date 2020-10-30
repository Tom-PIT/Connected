using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class Transaction : LongPrimaryKeyRecord, ITransaction
	{
		public int BlockCount { get; set; }
		public int BlockRemaining { get; set; }
		public Guid Partition { get; set; }
		public DateTime Created { get; set; }
		public Guid Token { get; set; }
		public TransactionStatus Status { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			BlockCount = GetInt("block_count");
			BlockRemaining = GetInt("block_remaining");
			Partition = GetGuid("partition_token");
			Created = GetDate("created");
			Token = GetGuid("token");
			Status = GetValue("status", TransactionStatus.Pending);
		}
	}
}
