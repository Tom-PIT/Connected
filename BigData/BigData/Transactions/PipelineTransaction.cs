using Amt.ComponentModel.Collections;
using Amt.Data;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Amt.DataHub.Transactions
{
	public abstract class PipelineTransaction
	{
		public const int FileSize = 10000000;
		private const int BlockSize = 10000;

		private DataTable _content = null;

		protected PipelineTransaction(DataTable content)
		{
			_content = content;
		}

		protected abstract ListItems<EndpointWorker> Workers { get; }

		private DataTable Content { get { return _content; } }

		public void Start()
		{
			var transaction = CreateDataBlocks();

			if (transaction > 0)
				ActivateTransaction(transaction);
		}

		private long CreateDataBlocks()
		{
			var dataTables = new List<DataTable>();
			int blocksCount = 0;
			var workers = new Dictionary<Guid, WorkerBase>();
			long transaction = 0;

			if (Content != null)
				blocksCount = CalculateBlockCount(Content.Rows.Count);

			if (blocksCount == 0)
				return 0;

			Parallel.Invoke(
					() =>
					{
						for (int i = 0; i < blocksCount; i++)
							dataTables.Add(CreateBlockTable(i, Content));

						if (blocksCount > 0)
							transaction = InsertTransaction(blocksCount);

					},
					() =>
					{
						if (Workers != null)
						{
							foreach (var i in Workers)
							{
								var worker = AmtShell.GetService<IConfigurationService>().Select<WorkerBase>(i.Worker);

								if (worker == null)
								{
									Log.Warning(this, "Worker not found", LogEvents.DhWorkerNull, i.Name, i.Worker.AsString());
									continue;
								}

								if (workers.ContainsKey(i.Worker))
								{
									Log.Warning(this, "More than one worker of the same type defined", LogEvents.DhWorkerDuplicate, i.Name, i.Worker.AsString());
									continue;
								}

								workers.Add(i.Worker, worker);
							}
						}
					});

			if (transaction == 0)
				return 0;

			try
			{
				var cs = new ContextState(null);

				Parallel.ForEach(dataTables,
						(t) =>
						{
							cs.Attach();

							CreateDataBlock(transaction, t, workers);
						});
			}
			catch (Exception ex)
			{
				Log.Error(this, ex, LogEvents.DhCreatingBlocks, transaction.AsString());

				transaction = 0;

				TryRollback(transaction);
			}

			return transaction;
		}

		private void CreateDataBlock(long transactionId, DataTable table, Dictionary<Guid, WorkerBase> workers)
		{
			var w = new Writer("amt_dh_transaction_block_ins");

			w.CreateParameter("@transaction_id", transactionId);
			w.CreateParameter("@worker_left", workers.Count);

			w.Execute();

			Parallel.Invoke(
					() =>
					{
						var block = DataHubModel.SelectTransactionBlock(w.Result);

						Storage.SaveBlock(block.Identifier, table);
					},
					() =>
					{
						var tbl = new DataTable();

						tbl.Columns.Add("block_id", typeof(long));
						tbl.Columns.Add("worker", typeof(Guid));
						tbl.Columns.Add("has_dependencies", typeof(bool));

						foreach (var i in workers)
						{
							DataRow row = tbl.NewRow();

							row["block_id"] = w.Result;
							row["worker"] = i.Key;
							row["has_dependencies"] = i.Value.Workers.Count > 0;

							tbl.Rows.Add(row);
						}

						var w2 = new Writer("amt_dh_transaction_task_ins");

						w2.CreateParameter("@workers", tbl);
						w2.Execute();
					}
					);
		}

		private void TryRollback(long transactionId)
		{
			try
			{
				var w = new Writer("amt_dh_transaction_del");

				w.CreateParameter("@id", transactionId);

				w.Execute();
			}
			catch (Exception ex)
			{
				Log.Error(this, ex, LogEvents.DhTransactionRollback, transactionId.AsString());
			}
		}

		private DataTable CreateBlockTable(int blockCount, DataTable table)
		{
			if (table == null)
				return null;

			var r = table.Clone();
			int startIndex = blockCount == 0 ? 0 : blockCount * BlockSize;
			int endIndex = startIndex + BlockSize;

			if (endIndex > table.Rows.Count)
				endIndex = table.Rows.Count;

			for (int i = startIndex; i < endIndex; i++)
				r.Rows.Add(table.Rows[i].ItemArray);

			return r;
		}

		private int CalculateBlockCount(int rows)
		{
			int remainder = rows % BlockSize;
			int blocks = rows / BlockSize;

			return remainder == 0 ? blocks : blocks + 1;
		}

		private long InsertTransaction(int blockCount)
		{
			try
			{
				var w = new Writer("amt_dh_transaction_ins");

				w.CreateParameter("@block_count", blockCount);

				w.Execute();

				return w.Result;
			}
			catch (Exception ex)
			{
				Log.Error(this, ex, LogEvents.DhCreateTransaction, blockCount.AsString());
				return 0;
			}
		}

		private void ActivateTransaction(long id)
		{
			var w = new Writer("amt_dh_transaction_activate");

			w.CreateParameter("@id", id);

			w.Execute();

		}
	}
}