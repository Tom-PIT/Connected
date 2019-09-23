using TomPIT.BigData.Data;

namespace TomPIT.BigData.Transactions
{
	internal interface IUpdateProvider
	{
		ITransactionBlock Block { get; }
		PartitionSchema Schema { get; }
	}
}
