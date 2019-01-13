using Amt.DataHub.Data;
using Amt.DataHub.Partitions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Amt.DataHub.Transactions
{
	public enum TransactionMode
	{
		InProcess = 1,
		Postponed = 2
	}

	public class PartitionTransaction
	{
		public const string TimestampColumn = "timestamp";

		private static Lazy<PartitionFileManager> _fileManager = new Lazy<PartitionFileManager>();

		public Partition Partition { get; set; }
		public long TaskId { get; set; }
		public Guid TaskPopReceipt { get; set; }
		public DataTable Table { get; set; }
		public TransactionMode Mode { get; set; } = TransactionMode.InProcess;
		public string Key { get; set; }
		public string KeyColumn { get; set; }

		public bool Execute()
		{
			List<DataFileContext> transactions = null;

			try
			{
				transactions = CreateDataFileContext();

				if (transactions == null || transactions.Count == 0)
				{
					Postpone();
					return false;
				}

				var merge = MergeData(transactions);

				if (!merge)
					return false;

				CheckDependencies();

				return true;
			}
			catch (Exception ex)
			{
				Log.Error(this, ex, LogEvents.DhTransactionExecute, Mode.ToString());

				if (PostponedData == null)
					Postpone();

				return false;
			}
			finally
			{
				ReleaseLocks(transactions);
			}
		}

		private void CheckDependencies()
		{
			var task = DataHubModel.SelectTransactionTask(TaskId);

			if (task == null || !task.HasDependencies)
				return;

			var worker = AmtShell.GetService<IConfigurationService>().Select<WorkerBase>(task.Worker);

			if (worker == null)
				return;

			MergeWithKey(Table);

#if DUMP
			Dump.CheckDependency(Partition.Id, Key, Table, TaskPopReceipt);
#endif

			PartitionModel.SaveWorkerData(Partition.Identifier, worker.RefId, Table, TaskPopReceipt);
		}

		private List<DataFileContext> CreateDataFileContext()
		{
			var r = new List<DataFileContext>();

			var minValue = Table.Compute(string.Format("Min({0})", TimestampColumn), string.Empty);
			var maxValue = Table.Compute(string.Format("Max({0})", TimestampColumn), string.Empty);

			DateTime min = minValue == null || minValue == DBNull.Value ? DateTime.UtcNow : (DateTime)minValue;
			DateTime max = maxValue == null || maxValue == DBNull.Value ? DateTime.UtcNow : (DateTime)maxValue;
			/*
			 * performance gain
			 * we'll select all rows at once which targets our data and then
			 * operate on that set
			 */
			var files = PartitionModel.QueryFiles(Partition.Id, Key, min, max);

			foreach (var i in files)
			{
				var ctx = new DataFileContext();

				ctx.Data = Table.Clone();
				ctx.File = i;

				r.Add(ctx);
			}

			foreach (DataRow i in Table.Rows)
			{
				DateTime timestampValue = (DateTime)i[TimestampColumn];

				if (r.Count == 0)
				{
					var dfc = CreateDataFileContext(timestampValue);
					/*
					 * if context can't be acquired it is possible that we have a locking 
					 * issue. transaction will probably be postponed
					 */
					if (dfc == null)
					{
						ReleaseLocks(r);
						return null;
					}

					files.Add(dfc.File);
					r.Add(dfc);
				}
				/*
				 * now, we'll append row to and file that fits in the timestamp range. this way, we'll repeatedly call merge
				 * for every file until we find all updates. If updates still remain on the last block we'll call full merge
				 * with inserts as well
				 */
				var targetFiles = r.Where(f =>
					((f.File.Status == 1 || f.File.Status == 3) || f.File.StartTimestamp <= timestampValue)
					&& (f.File.EndTimestamp == DateTime.MinValue || f.File.EndTimestamp >= timestampValue));

				if (targetFiles.Count(f => f.File.Status == (int)PartitionFileStatus.Maintenance) > 0)
				{
					ReleaseLocks(r);
					return null;
				}

				if (targetFiles.Count() == 0)
				{
					var dfc = CreateDataFileContext(timestampValue);

					if (dfc == null)
					{
						ReleaseLocks(r);
						return null;
					}

					files.Add(dfc.File);
					r.Add(dfc);

					var tfs = new List<DataFileContext>();

					tfs.Add(dfc);

					targetFiles = tfs;
				}

				foreach (var j in targetFiles)
				{
					if (!j.Locked)
					{
						FileManager.Lock(j.File.Id);

						j.Locked = true;
					}

					j.Data.Rows.Add(i.ItemArray);
				}

				/*
				 * check if all files are full.
				 * in that case this could case the row to go awol, because no merge
				 * call would insert it because block is it already full and on
				 * full blocks only updates are allowed.
				 * we'll create a new empty bock instead and this will allow us to
				 * safely complete the last merge on empty block.
				 */
				bool crowded = true;

				foreach (var j in targetFiles)
				{
					if (j.File.Status == 1)
					{
						crowded = false;
						break;
					}
				}

				if (crowded)
				{
					var file = CreateDataFileContext(timestampValue, true);

					if (file == null)
					{
						ReleaseLocks(r);
						return null;
					}

					file.Data.Rows.Add(i.ItemArray);

					files.Add(file.File);
					r.Add(file);
				}
			}

			return r.OrderBy(f => f.File.Status).ThenBy(f => f.File.StartTimestamp).ToList();
		}

		private void ReleaseLocks(List<DataFileContext> items)
		{
			if (items == null)
				return;

			foreach (var i in items)
			{
				if (i.Locked)
				{
					FileManager.Release(i.File.Id);
					i.Locked = false;
				}
			}
		}

		private DataFileContext CreateDataFileContext(DateTime timestamp, bool force = false)
		{
			var fileId = FileManager.CreateFile(Partition.Id, Key, timestamp, force);

			if (fileId == 0)
			{
				Postpone();
				return null;
			}
			else
			{
				var dfc = new DataFileContext();

				var file = FileManager.Lock(fileId);

				dfc.Data = Table.Clone();
				dfc.File = file;
				dfc.Locked = true;

				return dfc;
			}
		}

		private void Postpone()
		{
			Postpone(Table);
		}

		private void Postpone(DataTable table)
		{
			MergeWithKey(table);

			PostponedData = table;
			//Storage.SavePostponed(transaction.FileId, table);
		}

		private void MergeWithKey(DataTable table)
		{
			if (!string.IsNullOrWhiteSpace(KeyColumn))
			{
				if (!table.Columns.Contains(KeyColumn))
				{
					table.Columns.Add(KeyColumn, typeof(string));

					if (!string.IsNullOrWhiteSpace(Key))
					{
						foreach (DataRow i in table.Rows)
							i[KeyColumn] = Key;
					}
				}
			}
		}
		public DataTable PostponedData { get; private set; }

		private static PartitionFileManager FileManager
		{
			get { return _fileManager.Value; }
		}

		private bool MergeData(List<DataFileContext> transactions)
		{
			for (int i = 0; i < transactions.Count; i++)
			{
				var transaction = transactions[i];

				if (transaction.File.Status == 2)
					RemoveUpdated(transactions, PartialMerge(transaction));
				else
				{
					if (i > 0)
						MergeRemainingRows(transaction, transactions);

					var dt = FullMerge(transaction);

					if (i < transactions.Count - 1)
						RemoveUpdated(transactions, dt);
				}
			}

			return true;
		}

		private void MergeRemainingRows(DataFileContext lastTransaction, List<DataFileContext> transactions)
		{
			foreach (var i in transactions)
			{
				if (lastTransaction == i)
					continue;

				foreach (DataRow j in i.Data.Rows)
				{
					if (!RowExists(lastTransaction.Data, j))
						lastTransaction.Data.Rows.Add(j.ItemArray);
				}
			}
		}

		private void RemoveUpdated(IEnumerable<DataFileContext> transactions, DataTable records)
		{
			if (records == null || records.Rows.Count == 0)
				return;

			foreach (DataRow i in records.Rows)
			{
				foreach (var j in transactions)
				{
					var removes = new List<DataRow>();

					foreach (DataRow k in j.Data.Rows)
					{
						if (CompareRows(i, k))
							removes.Add(k);
					}

					foreach (var k in removes)
						j.Data.Rows.Remove(k);
				}
			}
		}

		private bool RowExists(DataTable table, DataRow row)
		{
			foreach (DataRow i in table.Rows)
			{
				if (CompareRows(i, row))
					return true;
			}

			return false;
		}
		private bool CompareRows(DataRow a, DataRow b)
		{
			for (int i = 0; i < a.ItemArray.Length; i++)
			{
				if (System.Collections.Comparer.Default.Compare(a.ItemArray[i], b.ItemArray[i]) != 0)
					return false;
			}

			return true;
		}

		private DataTable FullMerge(DataFileContext context)
		{
			if (context.Data.Rows.Count == 0)
				return null;

			try
			{
				if (!context.Locked)
					FileManager.Lock(context.File.Id);

				var node = AmtShell.GetService<INodeService>().Select(context.File.NodeId);
				var w = new NodeReader<MergeResultRecord>(node, string.Format("merge_t_{0}", context.File.FileId.AsString()), CommandType.StoredProcedure);

				w.CreateParameter("@rows", context.Data);

				w.Execute();

				PartitionModel.UpdateFileStatistics(context.File.Id);

#if DUMP
				Dump.FullMerge(Partition.Id, Key, context.Data);
#endif

				var dt = context.Data.Clone();

				foreach (var i in w.Result)
				{
					var itemArray = new object[i.ItemArray.Length - 1];

					Array.Copy(i.ItemArray, 1, itemArray, 0, i.ItemArray.Length - 1);

					dt.Rows.Add(itemArray);
				}

				return dt;
			}
			finally
			{
				if (context.Locked)
				{
					FileManager.Release(context.File.Id);
					context.Locked = false;
				}
			}
		}

		private DataTable PartialMerge(DataFileContext context)
		{
			try
			{
				if (context.Data.Rows.Count == 0)
					return null;

				var node = AmtShell.GetService<INodeService>().Select(context.File.NodeId);
				var w = new NodeReader<MergeResultRecord>(node, string.Format("merge_p_{0}", context.File.FileId.AsString()), CommandType.StoredProcedure);

				w.CreateParameter("@rows", context.Data);

				w.Execute();

#if DUMP
				Dump.PartialMerge(Partition.Id, Key, context.Data);
#endif

				var dt = context.Data.Clone();

				foreach (var i in w.Result)
					dt.Rows.Add(i.ItemArray);

				return dt;
			}
			finally
			{
				if (context.Locked)
					FileManager.Release(context.File.Id);
			}
		}
	}
}