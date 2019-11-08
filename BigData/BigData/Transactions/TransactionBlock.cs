using System;

namespace TomPIT.BigData.Transactions
{
	internal class TransactionBlock : ITransactionBlock
	{
		public Guid Transaction { get; set; }

		public Guid Partition { get; set; }

		public Guid Token { get; set; }
	}
}
