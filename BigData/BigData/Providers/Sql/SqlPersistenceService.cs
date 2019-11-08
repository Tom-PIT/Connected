using System;
using System.Data;
using TomPIT.BigData.Data;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;

namespace TomPIT.BigData.Providers.Sql
{
	internal class SqlPersistenceService : IPersistenceService
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
			var w = new NodeReader<MergeResultRecord>(node, $"merge_p_{context.TableName()}", CommandType.StoredProcedure);

			w.CreateParameter("@rows", context.Data);

			w.Execute();

			UpdateStatistics(provider, node, context, false);

			var dt = context.Data.Clone();

			foreach (var i in w.Result)
				dt.Rows.Add(i.ItemArray);

			return dt;
		}

		private DataTable FullMerge(IUpdateProvider provider, INode node, DataFileContext context)
		{
			var w = new NodeReader<MergeResultRecord>(node, $"merge_t_{context.TableName()}", CommandType.StoredProcedure);

			w.CreateParameter("@rows", context.Data);

			w.Execute();

			UpdateStatistics(provider, node, context, false);

			var dt = context.Data.Clone();

			foreach (var i in w.Result)
			{
				var itemArray = new object[i.ItemArray.Length - 1];

				Array.Copy(i.ItemArray, 1, itemArray, 0, i.ItemArray.Length - 1);

				dt.Rows.Add(itemArray);
			}

			return dt;
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
	}
}