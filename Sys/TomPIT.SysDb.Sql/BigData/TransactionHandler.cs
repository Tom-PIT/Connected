using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TransactionHandler : ITransactionHandler
	{
		public void Delete(ITransaction transaction)
		{
			var w = new Writer("tompit.big_data_transaction_del");

			w.CreateParameter("@id", transaction.GetId());

			w.Execute();
		}

		public void DeleteBlock(ITransactionBlock block)
		{
			var w = new Writer("tompit.big_data_transaction_block_del");

			w.CreateParameter("@id", block.GetId());

			w.Execute();
		}

		public List<ITransactionBlock> Dequeue(List<IResourceGroup> resourceGroups, int count, DateTime nextVisible, DateTime date)
		{
			var a = new JArray();

			foreach (var i in resourceGroups)
			{
				a.Add(new JObject
				{
					{ "resource_group", Convert.ToInt32(i.GetId()) }
				});
			}

			var r = new Reader<TransactionBlock>("tompit.big_data_transaction_block_dequeue");

			r.CreateParameter("@resource_groups", a);
			r.CreateParameter("@next_visible", nextVisible);
			r.CreateParameter("@count", count);
			r.CreateParameter("@date", date);

			return r.Execute().ToList<ITransactionBlock>();
		}

		public void Insert(IPartition partition, Guid token, int blockCount, DateTime created)
		{
			var w = new Writer("tompit.big_data_transaction_ins");

			w.CreateParameter("@partition", partition.GetId());
			w.CreateParameter("@token", token);
			w.CreateParameter("@block_count", blockCount);
			w.CreateParameter("@created", created);

			w.Execute();
		}

		public void InsertBlock(ITransaction transaction, Guid token)
		{
			var w = new Writer("tompit.big_data_transaction_block_ins");

			w.CreateParameter("@transaction", transaction.GetId());
			w.CreateParameter("@token", token);

			w.Execute();
		}

		public List<ITransaction> Query()
		{
			return new Reader<Transaction>("tompit.big_data_transaction_que").Execute().ToList<ITransaction>();
		}

		public ITransaction Select(Guid token)
		{
			var r = new Reader<Transaction>("tompit.big_data_transaction_que");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(ITransaction transaction, int blockRemaining, TransactionStatus status)
		{
			var w = new Writer("tompit.big_data_transaction_upd");

			w.CreateParameter("@id", transaction.GetId());
			w.CreateParameter("@block_remaining", blockRemaining);
			w.CreateParameter("@status", status);

			w.Execute();
		}

		public void UpdateBlock(ITransactionBlock block, DateTime nextVisible)
		{
			var w = new Writer("tompit.big_data_transaction_block_upd");

			w.CreateParameter("@id", block.GetId());
			w.CreateParameter("@next_visible", nextVisible);

			w.Execute();
		}
	}
}
