using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TomPIT.BigData.Data;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagostics;
using TomPIT.Middleware;

namespace TomPIT.BigData.Providers.Sql
{
	internal class DatabaseSchema
	{
		private IPartitionConfiguration _partition = null;
		private PartitionSchema _schema = null;

		public DatabaseSchema(INode node, IPartitionFile file)
		{
			Node = node;
			File = file;
		}

		private INode Node { get; }
		private IPartitionFile File { get; }

		public void Update()
		{
			if (!TableExists)
				CreateTable();

			AlterTable();
			RecreateHelpers();
		}

		public string TableName { get { return string.Format("t_{0}", File.TableName()); } }
		public string PartialTableName { get { return string.Format("p_{0}", File.TableName()); } }

		private bool TableExists
		{
			get
			{
				return new NodeAdminScalarReader<int>(Node, string.Format(SqlStrings.TableExists, TableName), CommandType.Text).ExecuteScalar(0) == 1;
			}
		}

		private IPartitionConfiguration Partition
		{
			get
			{
				if (_partition == null)
					_partition = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(File.Partition) as IPartitionConfiguration;

				return _partition;
			}
		}

		private PartitionSchema Schema
		{
			get
			{
				if (_schema == null)
					_schema = new PartitionSchema(Partition);

				return _schema;
			}
		}

		private void AlterTable()
		{
			var cols = LoadExistingColumns();

			foreach (var i in cols)
			{
				if (IsSystemColumn(i.ColumnName))
					continue;

				var field = Schema.Fields.FirstOrDefault(f => string.Compare(f.Name, i.ColumnName, true) == 0);

				if (field == null)
					DropColumn(i);
			}

			cols = LoadExistingColumns();

			foreach (var i in Schema.Fields)
			{
				if (IsSystemColumn(i.Name))
					continue;

				var ec = cols.FirstOrDefault(f => string.Compare(f.ColumnName, i.Name, true) == 0);

				if (ec == null)
					CreateColumn(i);
				else
				{
					//create index if column has it, else drop index
					if (i.Index)
						new NodeAdminWriter(Node, string.Format(SqlStrings.CreateIndex, TableName, i.Name), CommandType.Text).Execute();
					else
						new NodeAdminWriter(Node, string.Format(SqlStrings.DropIndex, TableName, i.Name), CommandType.Text).Execute();
				}
			}
		}

		private void CreateColumn(PartitionSchemaField field)
		{
			string fieldName = field.Name.ToLowerInvariant();

			new NodeAdminWriter(Node, string.Format(SqlStrings.ColumnCreate, TableName, fieldName, ParseColumnMetaData(field)), System.Data.CommandType.Text).Execute();

			if (field.Index)
				new NodeAdminWriter(Node, string.Format(SqlStrings.CreateIndex, TableName, fieldName), CommandType.Text).Execute();
		}

		private void CreateTable()
		{
			new NodeAdminWriter(Node, string.Format(SqlStrings.TableCreate, TableName), CommandType.Text).Execute();
			new NodeAdminWriter(Node, string.Format(SqlStrings.TableAddTimestampDefault, TableName), CommandType.Text).Execute();

			//create index on timestamp field
			new NodeAdminWriter(Node, string.Format(SqlStrings.CreateIndex, TableName, Merger.TimestampColumn), CommandType.Text).Execute();
		}

		private List<ExistingColumn> LoadExistingColumns()
		{
			return new NodeAdminReader<ExistingColumn>(Node, string.Format(SqlStrings.ColumnQuery, TableName), CommandType.Text).Execute();
		}

		private void DropColumn(ExistingColumn col)
		{
			new NodeAdminWriter(Node, string.Format(SqlStrings.ColumnDrop, TableName, col.ColumnName.ToLowerInvariant()), CommandType.Text).Execute();
		}

		internal void Drop()
		{
			try
			{
				new NodeAdminWriter(Node, string.Format(SqlStrings.DropProcedure, TableName), CommandType.Text).Execute();
				new NodeAdminWriter(Node, string.Format(SqlStrings.DropProcedureUpdate, PartialTableName), CommandType.Text).Execute();
				new NodeAdminWriter(Node, string.Format(SqlStrings.DropTable, TableName), CommandType.Text).Execute();
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, "BigData");
			}
		}

		private string ParseColumnMetaData(PartitionSchemaField field)
		{
			if (field is PartitionSchemaStringField sf)
				return ParseStringColumnMetaData(sf);
			else if (field is PartitionSchemaNumberField nf)
				return ParseNumberColumnMetaData(nf);
			else if (field is PartitionSchemaBoolField bf)
				return ParseBoolColumnMetaData(bf);
			else if (field is PartitionSchemaDateField df)
				return ParseDateColumnMetaData(df);
			else
				throw new NotSupportedException();
		}

		private string ParseStringColumnMetaData(PartitionSchemaStringField field)
		{
			return string.Format("NVARCHAR({0}) NULL", field.Length);
		}

		private string ParseNumberColumnMetaData(PartitionSchemaNumberField field)
		{
			if (field.Type == typeof(byte))
				return "tinyint NULL";
			else if (field.Type == typeof(short))
				return "smallint NULL";
			else if (field.Type == typeof(int))
				return "int NULL";
			else if (field.Type == typeof(long))
				return "bigint NULL";
			else if (field.Type == typeof(float)
				|| field.Type == typeof(double)
				|| field.Type == typeof(decimal))
				return "float NULL";
			else
				throw new NotSupportedException();
		}


		private string ParseBoolColumnMetaData(PartitionSchemaBoolField field)
		{
			return "bit NULL";
		}

		private string ParseDateColumnMetaData(PartitionSchemaDateField field)
		{
			return "datetime2(7) NULL";
		}

		private void RecreateHelpers()
		{
			new NodeAdminWriter(Node, string.Format(SqlStrings.DropProcedure, TableName), CommandType.Text).Execute();
			new NodeAdminWriter(Node, string.Format(SqlStrings.DropProcedureUpdate, PartialTableName), CommandType.Text).Execute();

			var commandText = string.Format(SqlStrings.CreateMergeProcedure, TableName, CreateProcedureColumns(), CreateMergeOn(), CreateSetStatement(), CreateInsertFields(), CreateRowFields());

			new NodeAdminWriter(Node, commandText, CommandType.Text).Execute();

			commandText = string.Format(SqlStrings.CreateUpdateProcedure, PartialTableName, TableName, CreateProcedureColumns(), CreateMergeOn(), CreateSetStatement(), CreateInsertFields(), CreateRowFields());

			new NodeAdminWriter(Node, commandText, CommandType.Text).Execute();
		}

		private string CreateRowFields()
		{
			var result = new StringBuilder();

			foreach (var field in Schema.Fields)
			{
				result.Append($"{field.Name} {GetSqlType(field)},");
			}

			return result.ToString().TrimEnd(',');
		}

		private string CreateProcedureColumns()
		{
			var sb = new StringBuilder();

			sb.Append($"{Merger.TimestampColumn},");

			foreach (var i in Schema.Fields)
			{
				if (IsSystemColumn(i.Name))
					continue;

				sb.AppendFormat("{0},", i.Name);
			}

			return sb.ToString().TrimEnd(',');
		}

		private string CreateMergeOn()
		{
			var sb = new StringBuilder();

			sb.Append($"t.{Merger.TimestampColumn} = s.{Merger.TimestampColumn}");

			foreach (var i in Schema.Fields)
			{
				if (IsSystemColumn(i.Name))
					continue;

				if (i.Key)
					sb.AppendFormat(" AND t.{0} = s.{0}", i.Name);
			}

			return sb.ToString();
		}

		private string CreateSetStatement()
		{
			var sb = new StringBuilder();

			foreach (var i in Schema.Fields)
			{
				if (IsSystemColumn(i.Name))
					continue;

				if (i is PartitionSchemaNumberField)
				{
					if (((PartitionSchemaNumberField)i).Aggregate == AggregateMode.Sum)
						sb.AppendFormat("t.{0} = t.{0} + s.{0},", i.Name);
					else
						sb.AppendFormat("t.{0} = s.{0},", i.Name);
				}
				else
					sb.AppendFormat("t.{0} = s.{0},", i.Name);
			}

			return sb.ToString().TrimEnd(',');
		}

		private string CreateInsertFields()
		{
			var sb = new StringBuilder();

			sb.AppendFormat($"s.{Merger.TimestampColumn},");

			foreach (var i in Schema.Fields)
			{
				if (IsSystemColumn(i.Name))
					continue;

				sb.AppendFormat("s.{0},", i.Name);
			}

			return sb.ToString().TrimEnd(',');
		}

		private string GetSqlType(PartitionSchemaField field)
		{
			if (field is PartitionSchemaStringField sf)
				return string.Format("nvarchar ({0})", sf.Length);
			else if (field is PartitionSchemaNumberField nf)
			{
				if (nf.Type == typeof(byte))
					return "tinyint";
				else if (nf.Type == typeof(short))
					return "smallint";
				else if (nf.Type == typeof(int))
					return "int";
				else if (nf.Type == typeof(long))
					return "bigint";
				else if (nf.Type == typeof(float)
					|| nf.Type == typeof(double)
					|| nf.Type == typeof(decimal))
					return "float";
				else
					throw new NotSupportedException();
			}
			else if (field is PartitionSchemaBoolField)
				return "bit";
			else if (field is PartitionSchemaDateField)
				return "datetime2";
			else
				throw new NotSupportedException();
		}

		private bool IsSystemColumn(string name)
		{
			return string.Compare(name, Merger.TimestampColumn, true) == 0
				|| string.Compare(name, Merger.IdColumn, true) == 0;
		}
	}
}