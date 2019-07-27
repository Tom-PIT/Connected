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

		List<ITransaction> Query();
		ITransaction Select(Guid token);

		void InsertBlock(ITransaction transaction, Guid token);
		void DeleteBlock(ITransactionBlock block);
		ITransactionBlock SelectBlock(Guid token);
		List<ITransactionBlock> QueryBlocks(ITransaction transaction);
	}
}
