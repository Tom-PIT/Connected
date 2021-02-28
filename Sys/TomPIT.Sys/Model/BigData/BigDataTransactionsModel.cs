using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.BigData;

namespace TomPIT.Sys.Model.BigData
{
	internal class BigDataTransactionsModel : SynchronizedRepository<IServerTransaction, Guid>
	{
		public BigDataTransactionsModel(IMemoryCache container) : base(container, "bigdatatransactions")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public void Activate(Guid token)
		{
			var transaction = Select(token);

			if (transaction == null)
				throw new SysException(SR.ErrBigDataTransactionNotFound);

			if (transaction.Status == TransactionStatus.Running)
				throw new SysException(SR.ErrBigDataTransactionRunning);

			var blocks = DataModel.BigDataTransactionBlocks.Query(transaction.Token);

			foreach (var block in blocks)
				DataModel.Queue.Enqueue(BigDataTransactionBlocksModel.Queue, block.Token.ToString(), TimeSpan.FromDays(14), TimeSpan.Zero, QueueScope.System);

			Update(transaction.Token, transaction.BlockRemaining, TransactionStatus.Running);
		}

		public IServerTransaction Select(Guid token)
		{
			return Get(token);
		}

		public List<IServerTransaction> Query()
		{
			return All();
		}

		public Guid Insert(Guid partition, int blockCount)
		{
			var part = DataModel.BigDataPartitions.Select(partition);

			if (part == null)
				throw new SysException(SR.ErrBigDataPartitionNotFound);

			var token = Guid.NewGuid();
			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Insert(part, token, blockCount, DateTime.UtcNow);

			Refresh(token);

			return token;
		}

		public void DecreaseBlock(Guid transaction)
		{
			var tran = Select(transaction);

			if (tran == null)
				return;

			lock (tran)
			{
				tran.DecrementBlock();

				if (tran.BlockRemaining > 0)
					Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Update(tran, tran.BlockRemaining, TransactionStatus.Running);
				else
				{
					Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Delete(tran);
					Remove(transaction);
				}
			}
		}

		public void Update(Guid transaction, int blockRemaining, TransactionStatus status)
		{
			var tran = Select(transaction);

			if (tran == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Update(tran, blockRemaining, status);

			Refresh(tran.Token);
		}

		public void Delete(Guid transaction)
		{
			var tran = Select(transaction);

			if (tran == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.Delete(tran);

			Remove(tran.Token);
		}
	}
}
