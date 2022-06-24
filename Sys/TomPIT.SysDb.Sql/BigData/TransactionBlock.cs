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
		public Guid Timezone { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Transaction = GetGuid("transaction_token");
			Partition = GetGuid("partition_configuration");
			Token = GetGuid("token");
			Timezone = GetGuid("timezone_token");
		}
	}
}
