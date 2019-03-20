using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class BigDataTransactionBlocks
	{
		public BigDataTransactionBlocks()
		{
		}

		public ITransaction Select(Guid token)
		{
			return null;// return Get(token);
		}

		public List<ITransactionBlock> Dequeue()
		{
			//var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Transactionb.Query();
			return null;

		}

		public Guid Insert(Guid partition, int blockCount)
		{
			var part = DataModel.BigDataPartitions.Select(partition);

			if (part == null)
				throw new SysException(SR.ErrBigDataPartitionNotFound);

			var token = Guid.NewGuid();
			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Insert(part, token, blockCount, DateTime.UtcNow);

			//Refresh(token);

			return token;
		}

		public void Update(Guid transaction, int blockRemaining, TransactionStatus status)
		{
			var tran = Select(transaction);

			if (tran == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Update(tran, blockRemaining, status);

			//Refresh(tran.Token);
		}

		public void Delete(Guid transaction)
		{
			var tran = Select(transaction);

			if (tran == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Delete(tran);

			//Remove(tran.Token);
		}
	}
}
