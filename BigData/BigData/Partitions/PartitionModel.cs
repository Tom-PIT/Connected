using Amt.Api.Data;
using Amt.ComponentModel.Dev;
using Amt.Core.Diagnostics;
using Amt.DataHub.Caching;
using Amt.DataHub.Data;
using Amt.DataHub.Schemas;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Runtime;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amt.DataHub.Partitions
{
	public static class PartitionModel
	{
		private static object _syncPartition = new object();
		private const string Category = "Partition service";

		private static Lazy<PartitionsCache> _partitionsCache = new Lazy<PartitionsCache>();
		private static Lazy<PartitionFilesCache> _partitionFilesCache = new Lazy<PartitionFilesCache>();

		public static void ChangeFileStatus(long fileId, PartitionFileStatus status)
		{
			var w = new Writer("amt_dh_index_upd_status");

			w.CreateParameter("@id", fileId);
			w.CreateParameter("@status", status);

			w.Execute();

			FilesCache.Invalidate(fileId);
		}

		public static int InsertPartition(string name, Guid partition, PartitionStatus status)
		{
			var w = new Writer("amt_dh_partition_ins");

			w.CreateParameter("@status", status);
			w.CreateParameter("@name", name);
			w.CreateParameter("@partition", partition);

			w.Execute();

			return w.Result;
		}

		public static List<PartitionFile> QueryFiles(int partitionId, string key, DateTime startTimestamp, DateTime endTimestamp)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_que");

			r.CreateParameter("@partition_id", partitionId);
			r.CreateParameter("@key", key, true);
			r.CreateParameter("@start_timestamp", startTimestamp, true);
			r.CreateParameter("@end_timestamp", endTimestamp, true);

			return r.Execute();
		}

		public static List<PartitionFile> QueryFiles(int partitionId)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_que_all");

			r.CreateParameter("@partition_id", partitionId);

			return r.Execute();
		}

		public static List<PartitionFile> QueryFiles(int partitionId, DateTime startTimestamp, DateTime endTimestamp)
		{
			return QueryFiles(partitionId, null, startTimestamp, endTimestamp);
		}

		public static List<Partition> QueryPartitions()
		{
			return new Reader<Partition>("amt_dh_partition_que").Execute();
		}

		public static PartitionFile SelectFile(long id)
		{
			return FilesCache.Get(id);
		}

		public static Partition SelectPartition(Guid identifier)
		{
			return PartitionsCache.Get(identifier);
		}

		public static Partition SelectPartition(int id)
		{
			return PartitionsCache.Get(id);
		}

		public static PartitionConfiguration SelectPartitionConfiguration(int partitionId)
		{
			var partition = SelectPartition(partitionId);

			if (partition == null)
				return null;

			return AmtShell.GetService<IConfigurationService>().Select<PartitionConfiguration>(partition.Identifier);
		}

		public static void UpdatePartition(int id, string name)
		{
			var partition = SelectPartition(id);

			if (partition == null)
				return;

			var w = new Writer("amt_dh_partition_upd");

			w.CreateParameter("@id", id);
			w.CreateParameter("@name", name);

			w.Execute();

			PartitionsCache.Invalidate(id);
		}

		public static long InsertPartitionFile(int partitionId, int nodeId, string key, DateTime timestamp, bool force = false)
		{
			var w = new LongWriter("amt_dh_index_ins");

			w.CreateParameter("@partition_id", partitionId);
			w.CreateParameter("@node_id", nodeId);
			w.CreateParameter("@key", key, true);
			w.CreateParameter("@start_timestamp", timestamp);
			w.CreateParameter("@force", force);

			w.Execute();

			return w.Result;
		}

		public static void DeletePartitionFile(long id)
		{
			var file = SelectFile(id);

			if (file == null)
				return;

			try
			{
				var w = new Writer("amt_dh_index_del");

				w.CreateParameter("@id", id);

				w.Execute();
			}
			catch (Exception ex)
			{
				Log.Error(typeof(PartitionModel), ex, LogEvents.DhDropPartitionFileIndexError, file.FileId.AsString());
			}

			var node = AmtShell.GetService<INodeService>().Select(file.NodeId);

			if (node == null)
				return;

			var ds = new DatabaseSchema(node, file);

			ds.Drop();
		}

		public static void UpdateFileStatistics(long fileId)
		{
			var file = SelectFile(fileId);

			if (file == null)
				return;

			var node = AmtShell.GetService<INodeService>().Select(file.NodeId);

			if (node == null)
				return;

			var stats = GetFileStatistics(node, file.FileId);

			var w = new Writer("amt_dh_index_upd");

			w.CreateParameter("@id", fileId);
			w.CreateParameter("@count", stats.Count);

			var p = w.CreateParameter("@start_timestamp", stats.MinTimestamp);

			p.DbType = DbType.DateTime2;

			p = w.CreateParameter("@end_timestamp", stats.Count >= PipelineTransaction.FileSize ? (object)stats.MaxTimestamp : DBNull.Value);

			p.DbType = DbType.DateTime2;

			w.CreateParameter("@status", stats.Count >= PipelineTransaction.FileSize ? PartitionFileStatus.Closed : PartitionFileStatus.Open);

			w.Execute();

			var config = SelectPartitionConfiguration(file.PartitionId);

			if (config != null)
			{
				foreach (var i in config.Schema)
				{
					if (i.Index)
						UpdateFieldStatistics(node, fileId, file.FileId, i);
				}
			}

			FilesCache.Invalidate(fileId);
		}

		private static void UpdateFieldStatistics(INode node, long fileId, Guid fileIdentifier, SchemaField field)
		{
			var commandText = string.Format("SELECT MIN([{0}]) AS minv, MAX([{0}]) AS maxv FROM [t_{1}]", field.Name, fileIdentifier.AsString());

			var startString = string.Empty;
			var endString = string.Empty;
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;
			var startNumber = 0d;
			var endNumber = 0d;

			if (field is SchemaStringField)
			{
				var r = new NodeReader<FileFieldStatistics<string>>(node, commandText, CommandType.Text).ExecuteSingleRow();

				startString = r.Min;
				endString = r.Max;
			}
			else if (field is SchemaDateField)
			{
				var r = new NodeReader<FileFieldStatistics<DateTime>>(node, commandText, CommandType.Text).ExecuteSingleRow();

				startDate = r.Min;
				endDate = r.Max;
			}
			else if (field is SchemaNumberField)
			{
				switch (((SchemaNumberField)field).NumberType)
				{
					case NumberFieldType.Byte:
						var b = new NodeReader<FileFieldStatistics<byte>>(node, commandText, CommandType.Text).ExecuteSingleRow();

						startNumber = b.Min;
						endNumber = b.Max;
						break;
					case NumberFieldType.Short:
						var s = new NodeReader<FileFieldStatistics<short>>(node, commandText, CommandType.Text).ExecuteSingleRow();

						startNumber = s.Min;
						endNumber = s.Max;
						break;
					case NumberFieldType.Int:
						var i = new NodeReader<FileFieldStatistics<int>>(node, commandText, CommandType.Text).ExecuteSingleRow();

						startNumber = i.Min;
						endNumber = i.Max;
						break;
					case NumberFieldType.Long:
						var l = new NodeReader<FileFieldStatistics<long>>(node, commandText, CommandType.Text).ExecuteSingleRow();

						startNumber = l.Min;
						endNumber = l.Max;
						break;
					case NumberFieldType.Float:
						var f = new NodeReader<FileFieldStatistics<double>>(node, commandText, CommandType.Text).ExecuteSingleRow();

						startNumber = f.Min;
						endNumber = f.Max;
						break;
					default:
						break;
				}
			}
			else
				return;

			var w = new Writer("amt_dh_index_field_mdf");

			w.CreateParameter("@index_id", fileId);
			w.CreateParameter("@field_name", field.Name);
			w.CreateParameter("@start_string", startString, true);
			w.CreateParameter("@end_string", endString, true);
			w.CreateParameter("@start_date", startDate, true);
			w.CreateParameter("@end_date", endDate, true);
			w.CreateParameter("@start_number", startNumber, true);
			w.CreateParameter("@end_number", endNumber, true);

			w.Execute();
		}

		private static FileStatistics GetFileStatistics(INode node, Guid fileId)
		{
			var commandText = string.Format("SELECT MIN(timestamp) AS min_timestamp, MAX(timestamp) AS max_timestamp, COUNT(*) AS count FROM [t_{0}]", fileId.AsString());

			return new NodeReader<FileStatistics>(node, commandText, CommandType.Text).ExecuteSingleRow();
		}

		public static List<PartitionFile> QueryFilesForData(int partitionId, string key, DateTime startTimestamp, DateTime endTimestamp)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_que_data");

			r.CreateParameter("@partition_id", partitionId);
			r.CreateParameter("@key", key, true);
			r.CreateParameter("@start_timestamp", startTimestamp);
			r.CreateParameter("@end_timestamp", endTimestamp);

			return r.Execute();
		}

		public static List<PartitionFile> QueryFilesForData(int partitionId, string key, string fieldName, DateTime startTimestamp, DateTime endTimestamp, DateTime startDate, DateTime endDate)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_que_data_date");

			r.CreateParameter("@partition_id", partitionId);
			r.CreateParameter("@key", key, true);
			r.CreateParameter("@start_timestamp", startTimestamp, true);
			r.CreateParameter("@end_timestamp", endTimestamp, true);
			r.CreateParameter("@field_name", fieldName);
			r.CreateParameter("@start_date", startDate, true);
			r.CreateParameter("@end_date", endDate, true);

			return r.Execute();
		}

		public static List<PartitionFile> QueryFilesForData(int partitionId, string key, string fieldName, DateTime startTimestamp, DateTime endTimestamp, string startString, string endString)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_que_data_string");

			r.CreateParameter("@partition_id", partitionId);
			r.CreateParameter("@key", key, true);
			r.CreateParameter("@start_timestamp", startTimestamp, true);
			r.CreateParameter("@end_timestamp", endTimestamp, true);
			r.CreateParameter("@field_name", fieldName);
			r.CreateParameter("@start_string", startString, true);
			r.CreateParameter("@end_string", endString, true);

			return r.Execute();
		}

		public static List<PartitionFile> QueryFilesForData(int partitionId, string key, string fieldName, DateTime startTimestamp, DateTime endTimestamp, double startNumber, double endNumber)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_que_data_num");

			r.CreateParameter("@partition_id", partitionId);
			r.CreateParameter("@key", key, true);
			r.CreateParameter("@start_timestamp", startTimestamp, true);
			r.CreateParameter("@end_timestamp", endTimestamp, true);
			r.CreateParameter("@field_name", fieldName);
			r.CreateParameter("@start_number", startNumber, true);
			r.CreateParameter("@end_number", endNumber, true);

			return r.Execute();
		}

		public static Partition EnsurePartition(PartitionConfiguration config)
		{
			var p = SelectPartition(config.RefId);

			if (p == null)
			{
				lock (_syncPartition)
				{
					p = SelectPartition(config.RefId);

					if (p == null)
					{
						var component = AmtShell.GetService<IComponentService>().Select(config.RefId);

						p = SelectPartition(InsertPartition(component.Name, config.RefId, PartitionStatus.Active));
					}
				}
			}

			return p;
		}

		public static bool Update(Guid partitionId, long taskId, DataTable data, Guid taskPopReceipt)
		{
			return Update(partitionId, taskId, data, TransactionMode.InProcess, 0, taskPopReceipt);
		}

		public static bool Update(Guid partitionId, long taskId, DataTable data, TransactionMode mode, long postponedTransaction, Guid taskPopReceipt)
		{
			var config = AmtShell.GetService<IConfigurationService>().Select<PartitionConfiguration>(partitionId);

			if (config == null)
				return true;

			var partition = EnsurePartition(config);

			var groups = CreateGroups(config, data);

			if (groups == null)
				return false;

			for (int i = 0; i < groups.Count; i++)
				groups[groups.Keys.ElementAt(i)] = Utils.ValidateDataTable(partition, config, groups[groups.Keys.ElementAt(i)]);

			bool result = true;

			DataTable postponed = null;
			object sync = new object();

			var cs = new ContextState(null);

			Parallel.ForEach(groups, (i) =>
			//foreach (var i in groups)
			{
				cs.Attach();

				var transaction = new PartitionTransaction();

				transaction.Mode = mode;
				transaction.Partition = partition;
				transaction.TaskId = taskId;
				transaction.TaskPopReceipt = taskPopReceipt;
				transaction.Table = i.Value;
				transaction.Key = i.Key;
				transaction.KeyColumn = config.PartitionKey;

				if (!transaction.Execute())
				{
					lock (sync)
					{
						if (transaction.PostponedData != null)
						{
							if (postponed == null)
								postponed = transaction.PostponedData.Clone();

							foreach (DataRow r in transaction.PostponedData.Rows)
								postponed.Rows.Add(r.ItemArray);
						}
					}

					result = false;
				}
			});

			if (!result && postponed != null && postponed.Rows.Count > 0)
			{
				if (mode == TransactionMode.Postponed)
				{
					var pt = DataHubModel.SelectPostponedTransaction(postponedTransaction);
					Storage.SavePostponed(pt.FileId, postponed);
				}
				else
				{
					var transaction = DataHubModel.PostponeTransaction(partition.Id, taskId);
					var pt = DataHubModel.SelectPostponedTransaction(transaction);
					Storage.SavePostponed(pt.FileId, postponed);
				}
			}

			return result;
		}

		public static Dictionary<string, DataTable> CreateGroups(PartitionConfiguration partition, DataTable data)
		{
			var r = new Dictionary<string, DataTable>();

			if (string.IsNullOrWhiteSpace(partition.PartitionKey))
				r.Add(string.Empty, data);
			else
			{
				if (!data.Columns.Contains(partition.PartitionKey))
				{
					Log.Warning(typeof(PartitionModel), "Partition data does not contain PartitionKey column.", LogEvents.DhPartitionDataKeyNull, partition.RefId.AsString(), partition.PartitionKey);
					return null;
				}

				foreach (DataRow i in data.Rows)
				{
					var v = i[partition.PartitionKey];

					if (v == DBNull.Value)
						v = null;

					var sv = Convert.ToString(v, CultureInfo.InvariantCulture);

					if (sv == null)
						sv = string.Empty;

					DataTable dt = null;

					if (r.ContainsKey(sv))
						dt = r[sv];
					else
					{
						dt = data.Clone();
						r.Add(sv, dt);
					}

					dt.Rows.Add(i.ItemArray);
				}
			}

			return r;
		}

		public static void SaveWorkerData(Guid partition, Guid worker, DataTable data, Guid taskPopReceipt)
		{
			Guid fileId = Storage.SavePartitionData(data);

			var w = new LongWriter("amt_dh_worker_dependency_ins");

			w.CreateParameter("@worker", worker);
			w.CreateParameter("@partition", partition);
			w.CreateParameter("@task_pop_receipt", taskPopReceipt);

			w.Execute();

			var w1 = new LongWriter("amt_dh_worker_dependency_file_ins");

			w1.CreateParameter("@dependecy_id", w.Result);
			w1.CreateParameter("@file_id", fileId);

			w1.Execute();
		}

		public static DataTable LoadWorkerData(long id, out List<WorkerDependencyFile> files)
		{
			files = DataHubModel.QueryWorkerDependencyFile(id);

			if (files.Count == 0)
				return null;

			DataTable result = null;

			foreach (var item in files)
			{
				var data = Storage.LoadPartitionData(item.FileId);

				if (data == null)
					return null;

				if (result == null)
					result = data.Clone();

				result.Merge(data);
			}

			return result;
		}

		public static void SynchronizePartition()
		{
			new Writer("amt_dh_partition_sync").Execute();
		}

		public static void UpdatePartitionStatus(int id, int status)
		{
			var w = new Writer("amt_dh_partition_upd_status");

			w.CreateParameter("@id", id);
			w.CreateParameter("@status", status);

			w.Execute();
		}

		public static List<Partition> QueryDeletedPartitions()
		{
			return new Reader<Partition>("amt_dh_partition_que_deleted").Execute();
		}

		public static void DeletePartitions()
		{
			var partitions = QueryDeletedPartitions();

			foreach (var partition in partitions)
			{
				var files = PartitionModel.QueryFiles(partition.Id, DateTime.UtcNow, DateTime.UtcNow);

				try
				{
					foreach (var file in files)
					{
						var node = AmtShell.GetService<INodeService>().Select(file.NodeId);

						string tableName = string.Format("t_{0}", file.FileId.AsString());
						string partialTableName = string.Format("p_{0}", file.FileId.AsString());

						new NodeAdminWriter(node, string.Format(Strings.DropProcedure, tableName), CommandType.Text).Execute();
						new NodeAdminWriter(node, string.Format(Strings.DropProcedureUpdate, partialTableName), CommandType.Text).Execute();
						new NodeAdminWriter(node, string.Format(Strings.DropStruct, tableName), CommandType.Text).Execute();
						new NodeAdminWriter(node, string.Format(Strings.DropTable, tableName), CommandType.Text).Execute();
					}

					var w = new Writer("amt_dh_partition_del");

					w.CreateParameter("@id", partition.Id);

					w.Execute();
				}
				catch (Exception ex)
				{
					AmtShell.GetService<ILoggingService>().Error(Category, ex);
				}
			}
		}

		public static List<Partition> QueryPartitionsForSynchronize()
		{
			return new Reader<Partition>("amt_dh_partition_que_sync").Execute();
		}

		public static void SynchronizePartitions()
		{
			var partitions = QueryPartitionsForSynchronize();

			foreach (var partition in partitions)
			{
				try
				{
					UpdatePartitionStatus(partition.Id, (int)PartitionStatus.ExecuteMaintenance);

					var files = QueryFiles(partition.Id, DateTime.UtcNow, DateTime.UtcNow);

					var cs = new ContextState(null);

					Parallel.ForEach(files, (file) =>
					{
						cs.Attach();

						var node = AmtShell.GetService<INodeService>().Select(file.NodeId);

						var ds = new DatabaseSchema(node, file);

						ds.Update();
					});

					UpdatePartitionStatus(partition.Id, (int)PartitionStatus.Active);
				}
				catch (Exception ex)
				{
					UpdatePartitionStatus(partition.Id, (int)PartitionStatus.PlannedMaintenance);

					AmtShell.GetService<ILoggingService>().Error(Category, ex);
				}
			}
		}

		private static PartitionsCache PartitionsCache { get { return _partitionsCache.Value; } }
		private static PartitionFilesCache FilesCache { get { return _partitionFilesCache.Value; } }

		public static PartitionFile LockFile(long id)
		{
			var retry = 1;

			while (retry < 10)
			{
				var file = TryGetLock(id);

				if (file != null)
					return file;

				Thread.Sleep(1000);

				retry++;
			}

			throw new Exception("File lock can not be obtained.");
		}

		private static PartitionFile TryGetLock(long id)
		{
			var r = new Reader<PartitionFile>("amt_dh_index_lock");

			r.CreateParameter("@id", id);

			return r.ExecuteSingleRow();
		}

		public static void ReleaseLock(long id)
		{
			var w = new Writer("amt_dh_index_release_lock");

			w.CreateParameter("@id", id);

			w.Execute();
		}
	}
}