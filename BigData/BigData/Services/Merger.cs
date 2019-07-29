using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Services
{
	internal class Merger
	{
		public const string TimestampColumn = "timestamp";
		public const string IdColumn = "id";

		private static readonly Lazy<PartitionFileManager> _fileManager = new Lazy<PartitionFileManager>();

		public Merger(IUpdateProvider provider, string partitionKey, DataTable data)
		{
			Provider = provider;
			PartitionKey = partitionKey;
			Data = data;
		}

		private string PartitionKey { get; }
		private IUpdateProvider Provider { get; }
		private DataTable Data { get; }
		public DataTable Locked { get; set; }
		private static PartitionFileManager FileManager => _fileManager.Value;

		public void Merge()
		{
			List<DataFileContext> transactions = null;

			try
			{
				transactions = CreateDataFileContext();

				if (transactions == null || transactions.Count == 0)
					return;

				Merge(transactions);
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("BigData", ex.Source, ex.Message);
			}
			finally
			{
				ReleaseLocks(transactions);
			}
		}

		private List<DataFileContext> CreateDataFileContext()
		{
			var r = new List<DataFileContext>();

			var minValue = Data.Compute(string.Format("Min({0})", TimestampColumn), string.Empty);
			var maxValue = Data.Compute(string.Format("Max({0})", TimestampColumn), string.Empty);

			DateTime min = minValue == null || minValue == DBNull.Value ? DateTime.UtcNow : (DateTime)minValue;
			DateTime max = maxValue == null || maxValue == DBNull.Value ? DateTime.UtcNow : (DateTime)maxValue;
			/*
			 * performance gain
			 * we'll select all rows at once which targets our data and then
			 * operate on that set
			 */
			var files = Instance.GetService<IPartitionService>().QueryFiles(Provider.Block.Partition, Provider.Schema.PartitionKeyField, min, max);

			foreach (var i in files)
			{
				var ctx = new DataFileContext
				{
					Data = Data.Clone(),
					File = i
				};

				r.Add(ctx);
			}

			foreach (DataRow i in Data.Rows)
			{
				var timestampValue = (DateTime)i[TimestampColumn];

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
				 * now, we'll append row to the file that fits in the timestamp range. this way, we'll repeatedly call merge
				 * for every file until we find all updates. If updates still remain on the last block we'll call full merge
				 * with inserts as well
				 */
				var targetFiles = r.Where(f =>
					((f.File.Status == PartitionFileStatus.Open) || f.File.StartTimestamp <= timestampValue)
					&& (f.File.EndTimestamp == DateTime.MinValue || f.File.EndTimestamp >= timestampValue));

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

					var tfs = new List<DataFileContext>
					{
						dfc
					};

					targetFiles = tfs;
				}

				foreach (var j in targetFiles)
				{
					if (j.Lock == Guid.Empty)
					{
						j.Lock = FileManager.Lock(j.File.FileName);

						if (j.Lock == Guid.Empty)
						{
							ReleaseLocks(r);
							return null;
						}
					}

					j.Data.Rows.Add(i.ItemArray);
				}

				/*
				 * check if all files are full.
				 * in that case this could case the row to go awol, because no merge
				 * call would insert it because block is already full and on
				 * full blocks only updates are allowed.
				 * we'll create a new empty bock instead and this will allow us to
				 * safely complete the last merge on empty block.
				 */
				bool full = true;

				foreach (var j in targetFiles)
				{
					if (j.File.Status == PartitionFileStatus.Open)
					{
						full = false;
						break;
					}
				}

				if (full)
				{
					var file = CreateDataFileContext(timestampValue);

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

		private DataFileContext CreateDataFileContext(DateTime timestamp)
		{
			var fileId = FileManager.CreateFile(Provider.Block.Partition, PartitionKey, timestamp);

			if (fileId == Guid.Empty)
			{
				SetLocked(Data.Rows);
				return null;
			}
			else
			{
				var dfc = new DataFileContext
				{
					Data = Data.Clone(),
					Lock = FileManager.Lock(fileId),
				};

				if (dfc.Lock == Guid.Empty)
					return null;

				dfc.File = Instance.GetService<IPartitionService>().SelectFile(fileId);

				return dfc;
			}
		}

		private void Merge(List<DataFileContext> transactions)
		{
			for (var i = 0; i < transactions.Count; i++)
			{
				var transaction = transactions[i];

				if (transaction.File.Status == PartitionFileStatus.Closed)
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
		}

		private DataTable FullMerge(DataFileContext context)
		{
			if (context.Data.Rows.Count == 0)
				return null;

			try
			{
				if (context.Lock == Guid.Empty)
					context.Lock = FileManager.Lock(context.File.FileName);

				if (context.Lock == Guid.Empty)
					return context.Data;

				var node = Instance.GetService<INodeService>().Select(context.File.Node);

				return Instance.GetService<IPersistenceService>().Merge(Provider, node, context, MergePolicy.Full);
			}
			finally
			{
				if (context.Lock != Guid.Empty)
				{
					FileManager.Release(context.Lock);
					context.Lock = Guid.Empty;
				}
			}
		}

		private DataTable PartialMerge(DataFileContext context)
		{
			try
			{
				if (context.Data.Rows.Count == 0)
					return null;

				var node = Instance.Connection.GetService<INodeService>().Select(context.File.Node);

				return Instance.GetService<IPersistenceService>().Merge(Provider, node, context, MergePolicy.Partial);
			}
			finally
			{
				if (context.Lock != Guid.Empty)
				{
					FileManager.Release(context.Lock);
					context.Lock = Guid.Empty;
				}
			}
		}

		private void ReleaseLocks(List<DataFileContext> items)
		{
			if (items == null)
				return;

			foreach (var i in items)
			{
				if (i.Lock!=Guid.Empty)
				{
					FileManager.Release(i.Lock);
					i.Lock = Guid.Empty;
				}
			}
		}

		private void SetLocked(DataRowCollection rows)
		{
			if (rows == null || rows.Count == 0)
				return;

			if (Locked == null)
				Locked = Data.Clone();

			foreach (DataRow i in rows)
				Locked.Rows.Add(i.ItemArray);
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

		private bool CompareRows(DataRow a, DataRow b)
		{
			for (var i = 0; i < a.ItemArray.Length; i++)
			{
				if (Comparer.Default.Compare(a.ItemArray[i], b.ItemArray[i]) != 0)
					return false;
			}

			return true;
		}

		private void MergeRemainingRows(DataFileContext lastTransaction, List<DataFileContext> transactions)
		{
			foreach (var transaction in transactions)
			{
				if (lastTransaction == transaction)
					continue;

				foreach (DataRow j in transaction.Data.Rows)
				{
					if (!RowExists(lastTransaction.Data, j))
						lastTransaction.Data.Rows.Add(j.ItemArray);
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
	}
}
