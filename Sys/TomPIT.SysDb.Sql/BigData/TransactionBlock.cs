using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TransactionBlock : LongPrimaryKeyRecord, ITransactionBlock
	{
		public Guid Transaction { get; set; }
		public Guid Partition { get; set; }
		public Guid Token { get; set; }
		public DateTime NextVisible { get; set; }
		public Guid PopReceipt { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Transaction = GetGuid("transaction_token");
			Partition = GetGuid("partition_configuration");
			Token = GetGuid("token");
			NextVisible = GetDate("next_visible");
			PopReceipt = GetGuid("pop_receipt");
		}
	}
}
