using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data.BigData
{
	internal class BigDataTransactionBlocks
	{
		public const string Queue = "bigdata";

		public BigDataTransactionBlocks()
		{
		}

		public ITransactionBlock Select(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.SelectBlock(token);
		}

		public List<IQueueMessage> Dequeue(int count, TimeSpan nextVisible)
		{
			return DataModel.Queue.Dequeue(count, nextVisible, QueueScope.System, Queue);
		}

		public List<ITransactionBlock> Query(Guid transaction)
		{
			var t = DataModel.BigDataTransactions.Select(transaction);

			if (t == null)
				throw new SysException(SR.ErrBigDataTransactionNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.QueryBlocks(t);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e != null)
			{
				Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.DeleteBlock(e);

				var transaction = DataModel.BigDataTransactions.Select(e.Transaction);

				lock (transaction)
				{
					if (transaction.BlockRemaining < 2)
						DataModel.BigDataTransactions.Delete(e.Transaction);
					else
						DataModel.BigDataTransactions.Update(transaction.Token, transaction.BlockRemaining - 1, TransactionStatus.Running);
				}
			}

			DataModel.Queue.Complete(popReceipt);
		}

		private ITransactionBlock Resolve(IQueueMessage message)
		{
			var id = new Guid(message.Message);

			return Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.SelectBlock(id);
		}

		public Guid Insert(Guid transaction)
		{
			var t = DataModel.BigDataTransactions.Select(transaction);

			if (t == null)
				throw new SysException(SR.ErrBigDataTransactionNotFound);

			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.BigData.Transactions.InsertBlock(t, token);

			return token;
		}
	}
}
