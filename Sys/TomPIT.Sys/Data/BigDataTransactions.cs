using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class BigDataTransactions : SynchronizedRepository<ITransaction, Guid>
	{
		public BigDataTransactions(IMemoryCache container) : base(container, "bigdatatransactions")
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

		public ITransaction Select(Guid token)
		{
			return Get(token);
		}

		public List<ITransaction> Query()
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
