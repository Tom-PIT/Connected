using Amt.Api.Data;
using Amt.Sdk.DataHub;
using System;

namespace Amt.DataHub.Transactions
{
	public class TransactionTask : LongPrimaryKeyRecord
	{
		public long BlockId { get; private set; }

		public Guid Identifier { get; private set; }
		public TransactionTaskStatus Status { get; private set; }

		public Guid Worker { get; private set; }

		public DateTime NextVisible { get; private set; }
		public bool HasDependencies { get; private set; }
		public Guid PopReceipt { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			BlockId = GetLong("block_id");
			Identifier = GetGuid("identifier");
			Status = GetValue("status", TransactionTaskStatus.Busy);
			Worker = GetGuid("worker");
			NextVisible = GetDate("next_visible");
			HasDependencies = GetBool("has_dependencies");
			PopReceipt = GetGuid("pop_receipt");
		}
	}
}