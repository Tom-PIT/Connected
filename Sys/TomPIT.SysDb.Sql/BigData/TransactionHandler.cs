using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TransactionHandler : ITransactionHandler
	{
		public void Delete(ITransaction transaction)
		{
			using var w = new Writer("tompit.big_data_transaction_del");

			w.CreateParameter("@id", transaction.GetId());

			w.Execute();
		}

		public void DeleteBlock(ITransactionBlock block)
		{
			using var w = new Writer("tompit.big_data_transaction_block_del");

			w.CreateParameter("@id", block.GetId());

			w.Execute();
		}

		public void Insert(IPartition partition, Guid token, int blockCount, DateTime created)
		{
			using var w = new Writer("tompit.big_data_transaction_ins");

			w.CreateParameter("@partition", partition.GetId());
			w.CreateParameter("@token", token);
			w.CreateParameter("@block_count", blockCount);
			w.CreateParameter("@created", created);

			w.Execute();
		}

		public void InsertBlock(ITransaction transaction, Guid token)
		{
			using var w = new Writer("tompit.big_data_transaction_block_ins");

			w.CreateParameter("@transaction", transaction.GetId());
			w.CreateParameter("@token", token);

			w.Execute();
		}

		public List<IServerTransaction> Query()
		{
			using var r = new Reader<Transaction>("tompit.big_data_transaction_que");

			return r.Execute().ToList<IServerTransaction>();
		}

		public IServerTransaction Select(Guid token)
		{
			using var r = new Reader<Transaction>("tompit.big_data_transaction_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(ITransaction transaction, int blockRemaining, TransactionStatus status)
		{
			using var w = new Writer("tompit.big_data_transaction_upd");

			w.CreateParameter("@id", transaction.GetId());
			w.CreateParameter("@block_remaining", blockRemaining);
			w.CreateParameter("@status", status);

			w.Execute();
		}

		public ITransactionBlock SelectBlock(Guid token)
		{
			using var r = new Reader<TransactionBlock>("tompit.big_data_transaction_block_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public List<ITransactionBlock> QueryBlocks(ITransaction transaction)
		{
			using var r = new Reader<TransactionBlock>("tompit.big_data_transaction_block_que");

			r.CreateParameter("@transaction", transaction.GetId());

			return r.Execute().ToList<ITransactionBlock>();
		}
	}
}
