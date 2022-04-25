using System;
using System.Collections.Generic;
using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface ITransactionHandler
	{
		void Insert(IPartition partition, ITimezone timezone, Guid token, int blockCount, DateTime created);
		void Update(ITransaction transaction, int blockRemaining, TransactionStatus status);
		void Delete(ITransaction transaction);

		List<IServerTransaction> Query();
		IServerTransaction Select(Guid token);

		void InsertBlock(ITransaction transaction, Guid token);
		void DeleteBlock(ITransactionBlock block);
		ITransactionBlock SelectBlock(Guid token);
		List<ITransactionBlock> QueryBlocks(ITransaction transaction);
	}
}
