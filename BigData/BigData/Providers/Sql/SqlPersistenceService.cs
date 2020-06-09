using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.BigData;
using TomPIT.BigData.Data;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.BigData.Providers.Sql
{
	internal class SqlPersistenceService : MiddlewareObject, IPersistenceService
	{
		public DataTable Merge(IUpdateProvider provider, INode node, DataFileContext context, MergePolicy policy)
		{
			switch (policy)
			{
				case MergePolicy.Partial:
					return PartialMerge(provider, node, context);
				case MergePolicy.Full:
					return FullMerge(provider, node, context);
				default:
					throw new NotSupportedException();
			}
		}

		private DataTable PartialMerge(IUpdateProvider provider, INode node, DataFileContext context)
		{
			var w = new NodeReader<MergeResultRecord>(node, CreateCommandText(provider, context, false), CommandType.StoredProcedure);

			w.CreateParameter("@rows", CreateParameter(context.Data));

			w.Execute();

			UpdateStatistics(provider, node, context, false);

			var dt = context.Data.Clone();

			foreach (var i in w.Result)
				MergeResultRecord(dt, i);

			return dt;
		}

		private DataTable FullMerge(IUpdateProvider provider, INode node, DataFileContext context)
		{
			var w = new NodeReader<MergeResultRecord>(node, CreateCommandText(provider, context, true), CommandType.Text);

			w.CreateParameter("@rows", CreateParameter(context.Data));

			w.Execute();

			UpdateStatistics(provider, node, context, false);

			var dt = context.Data.Clone();

			foreach (var i in w.Result)
				MergeResultRecord(dt, i);

			return dt;
		}

		private void MergeResultRecord(DataTable table, MergeResultRecord record)
		{
			var row = table.NewRow();

			foreach (var field in record.Fields)
			{
				var column = FindColumn(table, field.Name);

				if (column == null)
					continue;

				row[column] = field.Value;
			}

			table.Rows.Add(row);
		}

		private DataColumn FindColumn(DataTable table, string columnName)
		{
			foreach (DataColumn column in table.Columns)
			{
				if (string.Compare(column.ColumnName, columnName, true) == 0)
					return column;
			}

			return null;
		}

		private string CreateParameter(DataTable data)
		{
			var result = new JArray();

			foreach (DataRow row in data.Rows)
			{
				var item = new JObject();

				foreach (DataColumn column in data.Columns)
				{
					var value = row[column];

					if (value == null || value == DBNull.Value)
						continue;

					item.Add(new JProperty(column.ColumnName, value));
				}

				if (item.Count > 0)
					result.Add(item);
			}

			return Serializer.Serialize(result);
		}

		private string CreateCommandText(IUpdateProvider provider, DataFileContext context, bool fullMerge)
		{
			var result = new StringBuilder();

			result.AppendLine($"MERGE t_{context.TableName()} AS t");
			result.AppendLine("USING (SELECT * FROM OPENJSON(@rows) WITH (");
			var hit = false;

			foreach (var field in provider.Schema.Fields)
			{
				if (hit)
					result.Append(", ");

				hit = true;
				result.Append($"{field.Name} {ResolveFieldDataTypeString(field)}");
			}

			result.Append(")) AS s (");
			hit = false;

			foreach (var field in provider.Schema.Fields)
			{
				if (hit)
					result.Append(", ");

				hit = true;
				result.Append($"{field.Name}");
			}

			result.AppendLine(")");
			result.Append("ON (");
			hit = false;

			foreach (var field in provider.Schema.Fields)
			{
				if (!field.Key && string.Compare(Merger.TimestampColumn, field.Name, true) != 0)
					continue;

				if (hit)
					result.Append(" AND ");

				hit = true;
				result.Append($"t.{field.Name} = s.{field.Name}");
			}

			result.AppendLine(")");
			result.AppendLine("WHEN matched THEN");
			result.Append("UPDATE SET ");
			hit = false;

			foreach (var field in provider.Schema.Fields)
			{
				if (field.Key)
					continue;

				if (provider.Schema.Middleware.Timestamp == TimestampBehavior.Static && string.Compare(field.Name, Merger.TimestampColumn, true) == 0)
					continue;

				if (hit)
					result.Append(", ");

				hit = true;

				var aggregate = field.Attributes.FirstOrDefault(f => f is BigDataAggregateAttribute) as BigDataAggregateAttribute;

				if (aggregate != null)
				{
					switch (aggregate.Mode)
					{
						case AggregateMode.None:
							result.Append($"t.{field.Name} = s.{field.Name}");
							break;
						case AggregateMode.Sum:
							result.Append($"t.{field.Name} = t.{field.Name} + s.{field.Name}");
							break;
						default:
							throw new NotSupportedException();
					}
				}
				else
					result.Append($"t.{field.Name} = s.{field.Name}");
			}

			if (fullMerge)
			{
				result.AppendLine(" WHEN not matched THEN");
				result.Append("INSERT (");
				hit = false;

				foreach (var field in provider.Schema.Fields)
				{
					if (hit)
						result.Append(", ");

					hit = true;
					result.Append(field.Name);
				}

				result.Append(") VALUES (");
				hit = false;

				foreach (var field in provider.Schema.Fields)
				{
					if (hit)
						result.Append(", ");

					hit = true;
					result.Append($"s.{field.Name}");
				}

				result.AppendLine(")");
			}

			if (fullMerge)
				result.AppendLine("OUTPUT $action, inserted.*;");
			else
				result.AppendLine("OUTPUT inserted.*;");

			return result.ToString();
		}

		private string ResolveFieldDataTypeString(PartitionSchemaField field)
		{
			if (field is PartitionSchemaStringField sf)
				return $"nvarchar({sf.Length})";
			else if (field is PartitionSchemaNumberField nf)
				return $"float";
			else if (field is PartitionSchemaDateField df)
				return $"datetime2(7)";
			else if (field is PartitionSchemaBoolField bf)
				return $"bit";
			else
				throw new NotSupportedException();
		}

		public PartitionSchema SelectSchema(IPartitionConfiguration configuration)
		{
			return new PartitionSchema(configuration);
		}

		public void SynchronizeSchema(INode node, IPartitionFile file)
		{
			var schema = new DatabaseSchema(node, file);

			schema.Update();
		}

		private void UpdateStatistics(IUpdateProvider provider, INode node, DataFileContext context, bool fieldsOnly)
		{
			if (!fieldsOnly)
			{
				var stats = GetFileStatistics(node, context);

				var max = stats.Count >= TransactionParser.FileSize ? stats.MaxTimestamp : DateTime.MinValue;
				var status = stats.Count >= TransactionParser.FileSize ? PartitionFileStatus.Closed : PartitionFileStatus.Open;

				MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().UpdateFile(context.File.FileName, stats.MinTimestamp, max, stats.Count, status);
			}

			foreach (var field in provider.Schema.Fields)
			{
				if (field.Index)
					UpdateFieldStatistics(node, context.File, field);
			}
		}

		private FileStatistics GetFileStatistics(INode node, DataFileContext context)
		{
			var commandText = $"SELECT MIN({Merger.TimestampColumn}) AS min_timestamp, MAX({Merger.TimestampColumn}) AS max_timestamp, COUNT(*) AS count FROM [t_{context.TableName()}]";

			return new NodeReader<FileStatistics>(node, commandText, CommandType.Text).ExecuteSingleRow();
		}

		private static void UpdateFieldStatistics(INode node, IPartitionFile file, PartitionSchemaField field)
		{
			var commandText = string.Format("SELECT MIN([{0}]) AS minv, MAX([{0}]) AS maxv FROM [t_{1}]", field.Name, file.TableName());

			var startString = string.Empty;
			var endString = string.Empty;
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;
			var startNumber = 0d;
			var endNumber = 0d;

			if (field is PartitionSchemaStringField)
			{
				var r = new NodeReader<FileFieldStatistics<string>>(node, commandText, CommandType.Text).ExecuteSingleRow();

				startString = r.Min;
				endString = r.Max;
			}
			else if (field is PartitionSchemaDateField)
			{
				var r = new NodeReader<FileFieldStatistics<DateTime>>(node, commandText, CommandType.Text).ExecuteSingleRow();

				startDate = r.Min;
				endDate = r.Max;
			}
			else if (field is PartitionSchemaNumberField nf)
			{
				if (nf.Type == typeof(byte))
				{
					var b = new NodeReader<FileFieldStatistics<byte>>(node, commandText, CommandType.Text).ExecuteSingleRow();

					startNumber = b.Min;
					endNumber = b.Max;
				}
				else if (nf.Type == typeof(short))
				{
					var s = new NodeReader<FileFieldStatistics<short>>(node, commandText, CommandType.Text).ExecuteSingleRow();

					startNumber = s.Min;
					endNumber = s.Max;
				}
				else if (nf.Type == typeof(int))
				{
					var i = new NodeReader<FileFieldStatistics<int>>(node, commandText, CommandType.Text).ExecuteSingleRow();

					startNumber = i.Min;
					endNumber = i.Max;
				}
				else if (nf.Type == typeof(long))
				{
					var l = new NodeReader<FileFieldStatistics<long>>(node, commandText, CommandType.Text).ExecuteSingleRow();

					startNumber = l.Min;
					endNumber = l.Max;
				}
				else if (nf.Type == typeof(float) || nf.Type == typeof(double) || nf.Type == typeof(decimal))
				{
					var f = new NodeReader<FileFieldStatistics<double>>(node, commandText, CommandType.Text).ExecuteSingleRow();

					startNumber = f.Min;
					endNumber = f.Max;
				}
				else
					throw new NotSupportedException();
			}
			else
				return;

			MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().UpdateFileStatistics(file.FileName, field.Name, startString, endString, startNumber, endNumber, startDate, endDate);
		}

		public JArray Query(IPartitionConfiguration configuration, List<QueryParameter> parameters)
		{
			var query = new Query(Context, configuration, parameters);

			query.Execute();

			return query.Result;
		}
	}
}