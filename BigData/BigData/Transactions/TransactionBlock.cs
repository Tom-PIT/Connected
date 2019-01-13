using Amt.Api.Data;
using System;

namespace Amt.DataHub.Transactions
{
	public class TransactionBlock : LongPrimaryKeyRecord
	{
		public Guid Identifier { get; private set; }

		public long TransactionId { get; private set; }

		public int WorkerLeft { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Identifier = GetGuid("identifier");
			TransactionId = GetLong("transaction_id");
			WorkerLeft = GetInt("worker_left");
		}
	}
}
