using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class StorageWorkerItem
	{
		public StorageWorkerItem(ITransactionBlock block, IQueueMessage message)
		{
			Block = block;
			Message = message;
		}
		public ITransactionBlock Block { get; }
		public IQueueMessage Message { get; }
	}
}
