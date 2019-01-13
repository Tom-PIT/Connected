using Amt.Api.Common;
using Amt.DataHub.Partitions;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using Amt.Sdk.Exceptions;
using Amt.Sdk.Runtime;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Amt.DataHub.Data
{
	internal class DataHubQuery
	{
		private const int ConnectionLimit = 10;
		public const string StartTimestampParameter = "@StartTimestamp";
		public const string EndTimestampParameter = "@EndTimestamp";
		public const string KeyParameter = "@Key";

		private DataHubCommandTextParser _parser = null;
		private Partition _partition = null;
		private PartitionConfiguration _config = null;
		private List<Tuple<string, object>> _parameters = null;

		public DataHubQuery(string commandText, List<Tuple<string, object>> parameters)
		{
			_parameters = parameters;
			_parser = new DataHubCommandTextParser(commandText);

			if (_parser.Partition == Guid.Empty)
				return;

			_partition = PartitionModel.SelectPartition(_parser.Partition);

			ParseTimestamps();
		}

		private void ParseTimestamps()
		{
			if (_partition == null)
				return;

			if (_parameters == null || _parameters.Count == 0)
				return;

			var st = _parameters.FirstOrDefault(f => string.Compare(f.Item1, StartTimestampParameter, true) == 0);

			if (st != null && st.Item2 != null && st.Item2 != DBNull.Value && AmtTypeConverter.CanConvertTo<DateTime>(st.Item2))
				StartTimestamp = AmtTypeConverter.ConvertTo<DateTime>(st.Item2);

			var et = _parameters.FirstOrDefault(f => string.Compare(f.Item1, EndTimestampParameter, true) == 0);

			if (et != null && et.Item2 != null && et.Item2 != DBNull.Value && AmtTypeConverter.CanConvertTo<DateTime>(et.Item2))
				EndTimestamp = AmtTypeConverter.ConvertTo<DateTime>(et.Item2);

			if (StartTimestamp == DateTime.MinValue)
				StartTimestamp = _partition.Created;

			if (EndTimestamp == DateTime.MinValue)
				EndTimestamp = DateTime.UtcNow;

			var k = _parameters.FirstOrDefault(f => string.Compare(f.Item1, KeyParameter, true) == 0);

			if (k != null && k.Item2 != null && k.Item2 != DBNull.Value)
				Key = k.Item2.ToString();
		}

		public DataTable Execute()
		{
			if (_partition == null)
				return null;

			_config = AmtShell.GetService<IConfigurationService>().Select<PartitionConfiguration>(_partition.Identifier);

			if (_config == null)
				return null;

			var dataTable = CreateSchema(_config, _parser.Select);

			var files = GetPartitionFiles();

			var queue = new ConcurrentQueue<PartitionFile>();

			foreach (var i in files)
				queue.Enqueue(i);

			var workers = new List<DataHubQueryWorker>();

			int count = files.Count > ConnectionLimit ? ConnectionLimit : files.Count;

			for (int i = 0; i < count; i++)
				workers.Add(new DataHubQueryWorker());

			var ctx = new DataHubQueryContext();

			var cs = new ContextState(null);

			Parallel.ForEach(workers, (i) =>
			{
				cs.Attach();

				i.Execute(ctx, _parser, queue, _parameters, dataTable);
			});

			if (ctx.IsFull)
				throw new ApiException(string.Format("{0} ({1})", StringUtils.SR("ErrDataHubMaxRowExceeded"), DataHubQueryContext.RowLimit));

			foreach (var i in workers)
			{
				if (i.Result.Rows.Count > 0)
					dataTable.Merge(i.Result);
			}

			return dataTable;
		}

		private List<PartitionFile> GetPartitionFiles()
		{
			if (_parser.Set.Count == 0)
				return PartitionModel.QueryFilesForData(_partition.Id, Key, StartTimestamp, EndTimestamp);
			else
			{
				var criteria = ParseCriteria(_parser.Set);
				var files = new List<PartitionFile>();

				foreach (var set in criteria)
				{
					var field = _config.Schema.FirstOrDefault(f => string.Compare(set.Key, f.Name, true) == 0);

					if (!string.IsNullOrWhiteSpace(set.Field) && field == null)
						continue;

					if (string.IsNullOrWhiteSpace(set.Field))
						files.AddRange(PartitionModel.QueryFilesForData(_partition.Id, set.Key, set.StartTimestamp, set.EndTimestamp));
					else
					{
						if (field is SchemaStringField)
							files.AddRange(PartitionModel.QueryFilesForData(_partition.Id, Key, field.Name, set.StartTimestamp, set.EndTimestamp, set.MinString, set.MaxString));
						else if (field is SchemaDateField)
							files.AddRange(PartitionModel.QueryFilesForData(_partition.Id, Key, field.Name, set.StartTimestamp, set.EndTimestamp, set.MinDate, set.MaxDate));
						else if (field is SchemaNumberField)
							files.AddRange(PartitionModel.QueryFilesForData(_partition.Id, Key, field.Name, set.StartTimestamp, set.EndTimestamp, set.MinNumber, set.MaxNumber));
					}
				}

				//return distinct files
				return files.GroupBy(f => f.Id).Select(f => f.First()).ToList();
			}
		}

		private List<FileCriteria> ParseCriteria(Dictionary<string, Tuple<string, string>> statements)
		{
			var r = new List<FileCriteria>();

			if (statements == null)
				return r;

			foreach (var i in statements)
			{
				if (string.Compare(i.Key, "Key", true) == 0)
				{
					if (string.IsNullOrWhiteSpace(i.Value.Item1))
						continue;

					var tokens = Key.Split(';');

					foreach (var j in tokens)
					{
						if (string.IsNullOrWhiteSpace(j))
							continue;

						var existing = r.FirstOrDefault(f => string.Compare(f.Key, j, true) == 0);

						if (existing == null)
						{
							existing = new FileCriteria();

							existing.Key = j;

							r.Add(existing);
						}
					}
				}
			}

			var rr = new List<FileCriteria>();

			foreach (var i in statements)
			{
				if (string.Compare(i.Key, "Key", true) == 0
					|| string.Compare(i.Key, "Timestamp", true) == 0)
					continue;

				var field = _config.Schema.FirstOrDefault(f => string.Compare(i.Key, f.Name, true) == 0);

				if (field == null)
					continue;

				if (r.Count == 0)
				{
					var fc = new FileCriteria();

					fc.Key = i.Key;
					fc.Field = i.Key;

					SetCriteriaValues(fc, field, i.Value);

					r.Add(fc);
				}
				else
				{
					foreach (var j in r)
					{
						var fc = new FileCriteria();

						fc.Key = i.Key;
						fc.Field = j.Key;

						SetCriteriaValues(fc, field, i.Value);

						rr.Add(fc);
					}
				}
			}

			if (rr.Count > 0)
				r = rr;

			foreach (var i in statements)
			{
				if (string.Compare(i.Key, "Timestamp", true) == 0)
				{
					var min = string.IsNullOrWhiteSpace(i.Value.Item1) ? DateTime.MinValue : Convert.ToDateTime(StartTimestamp);
					var max = string.IsNullOrWhiteSpace(i.Value.Item2) ? DateTime.MinValue : Convert.ToDateTime(EndTimestamp);

					if (r.Count == 0)
					{
						var fc = new FileCriteria();

						fc.StartTimestamp = min;
						fc.EndTimestamp = max;

						r.Add(fc);
					}
					else
					{
						foreach (var j in r)
						{
							j.StartTimestamp = min;
							j.EndTimestamp = max;
						}
					}
				}
			}

			return r;
		}

		private void SetCriteriaValues(FileCriteria criteria, SchemaField field, Tuple<string, string> value)
		{
			var tupleMin = _parameters.FirstOrDefault(f => string.Compare(f.Item1, value.Item1, true) == 0);
			var tupleMax = _parameters.FirstOrDefault(f => string.Compare(f.Item1, value.Item2, true) == 0);

			if (field is SchemaStringField)
			{
				criteria.MinString = (string)tupleMin.Item2;
				criteria.MaxString = (string)tupleMax.Item2;
			}
			else if (field is SchemaDateField)
			{
				criteria.MinDate = tupleMin.Item2 == null || tupleMin.Item2 == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(tupleMin.Item2);
				criteria.MaxDate = tupleMin.Item2 == null || tupleMin.Item2 == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(tupleMax.Item2);
			}
			else if (field is SchemaNumberField)
			{
				criteria.MinNumber = tupleMin.Item2 == null || tupleMin.Item2 == DBNull.Value ? 0d : Convert.ToDouble(value.Item1);
				criteria.MaxNumber = tupleMin.Item2 == null || tupleMin.Item2 == DBNull.Value ? 0d : Convert.ToDouble(value.Item2);
			}
		}

		private DateTime StartTimestamp
		{
			get; set;
		}

		private DateTime EndTimestamp
		{
			get; set;
		}

		private string Key
		{
			get; set;
		}
		public static DataTable CreateSchema(PartitionConfiguration config, string selectCommandText)
		{
			var fields = selectCommandText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			var r = new DataTable();

			foreach (var i in fields)
			{
				if (string.Compare(i, PartitionTransaction.TimestampColumn, true) == 0)
					r.Columns.Add(PartitionTransaction.TimestampColumn, typeof(DateTime));
				else
				{
					var field = config.Schema.FirstOrDefault(f => string.Compare(i, f.Name, true) == 0);

					if (field != null)
						r.Columns.Add(field.Name, ResolveType(field));
				}
			}

			return r;
		}

		public static DataTable CreateSchema(PartitionConfiguration config)
		{
			var r = new DataTable();

			r.Columns.Add(PartitionTransaction.TimestampColumn, typeof(DateTime));

			foreach (var i in config.Schema)
				r.Columns.Add(i.Name, ResolveType(i));

			return r;
		}

		private static Type ResolveType(SchemaField field)
		{
			if (field is SchemaStringField)
				return typeof(string);
			else if (field is SchemaNumberField)
			{
				var nf = field as SchemaNumberField;

				switch (nf.NumberType)
				{
					case NumberFieldType.Byte:
						return typeof(byte);
					case NumberFieldType.Short:
						return typeof(short);
					case NumberFieldType.Int:
						return typeof(int);
					case NumberFieldType.Long:
						return typeof(long);
					case NumberFieldType.Float:
						return typeof(float);
					default:
						throw new NotSupportedException();
				}
			}
			else if (field is SchemaBoolField)
				return typeof(bool);
			else if (field is SchemaDateField)
				return typeof(DateTime);
			else
				throw new NotSupportedException();
		}

	}
}