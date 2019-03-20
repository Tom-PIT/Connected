using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Environment;

namespace TomPIT.SysDb.BigData
{
	public interface ITransactionHandler
	{
		void Insert(IPartition partition, Guid token, int blockCount, DateTime created);
		void Update(ITransaction transaction, int blockRemaining, TransactionStatus status);
		void Delete(ITransaction transaction);

		void InsertBlock(ITransaction transaction, Guid token);
		List<ITransaction> Query();
		List<ITransactionBlock> Dequeue(List<IResourceGroup> resourceGroups, int count, DateTime nextVisible, DateTime date);
		void DeleteBlock(ITransactionBlock block);
		void UpdateBlock(ITransactionBlock block, DateTime nextVisible);
		ITransaction Select(Guid token);
	}
}
