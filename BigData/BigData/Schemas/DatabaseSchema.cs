using Amt.DataHub.Data;
using Amt.DataHub.Partitions;
using Amt.Sdk.DataHub;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Amt.DataHub.Schemas
{
	internal class DatabaseSchema
	{
		private INode _node = null;
		private PartitionFile _file = null;
		private PartitionConfiguration _partition = null;

		public DatabaseSchema(INode node, PartitionFile file)
		{
			_node = node;
			_file = file;
		}

		public void Update()
		{
			if (!TableExists)
				CreateTable();

			AlterTable();
			RecreateHelpers();
		}

		public string TableName { get { return string.Format("t_{0}", _file.FileId.AsString()); } }
		public string PartialTableName { get { return string.Format("p_{0}", _file.FileId.AsString()); } }

		private bool TableExists
		{
			get
			{
				return new NodeAdminScalarReader<int>(_node,
						string.Format(Strings.TableExists, TableName),
						System.Data.CommandType.Text).ExecuteScalar(0) == 1;
			}
		}

		private PartitionConfiguration Partition
		{
			get
			{
				if (_partition == null)
					_partition = PartitionModel.SelectPartitionConfiguration(_file.PartitionId);

				return _partition;
			}
		}

		private void AlterTable()
		{
			var cols = LoadExistingColumns();

			foreach (var i in cols)
			{
				var field = Partition.Schema.FirstOrDefault(f => string.Compare(f.Name, i.ColumnName, true) == 0);

				if (field == null)
					DropColumn(i);
			}

			cols = LoadExistingColumns();

			foreach (var i in Partition.Schema)
			{
				ExistingColumn ec = cols.FirstOrDefault(f => string.Compare(f.ColumnName, i.Name, true) == 0);

				if (ec == null)
					CreateColumn(i);
				else
				{
					//create index if column has it, else drop index
					if (i.Index)
						new NodeAdminWriter(_node, string.Format(Strings.CreateIndex, TableName, i.Name), CommandType.Text).Execute();
					else
						new NodeAdminWriter(_node, string.Format(Strings.DropIndex, TableName, i.Name), CommandType.Text).Execute();
				}
			}
		}

		private void CreateColumn(SchemaField field)
		{
			string fieldName = field.Name.ToLowerInvariant();

			new NodeAdminWriter(_node, string.Format(Strings.ColumnCreate, TableName, fieldName, ParseColumnMetaData(field)), System.Data.CommandType.Text).Execute();

			if (field.Index)
				new NodeAdminWriter(_node, string.Format(Strings.CreateIndex, TableName, fieldName), CommandType.Text).Execute();
		}

		private void CreateTable()
		{
			new NodeAdminWriter(_node, string.Format(Strings.TableCreate, TableName), CommandType.Text).Execute();
			new NodeAdminWriter(_node, string.Format(Strings.TableAddTimestampDefault, TableName), CommandType.Text).Execute();

			//create index on timestamp field
			new NodeAdminWriter(_node, string.Format(Strings.CreateIndex, TableName, "timestamp"), CommandType.Text).Execute();
		}

		private List<ExistingColumn> LoadExistingColumns()
		{
			return new NodeAdminReader<ExistingColumn>(_node, string.Format(Strings.ColumnQuery, TableName), CommandType.Text).Execute();
		}

		private void DropColumn(ExistingColumn col)
		{
			new NodeAdminWriter(_node, string.Format(Strings.ColumnDrop, TableName, col.ColumnName.ToLowerInvariant()), CommandType.Text).Execute();
		}

		private bool AlterColumn(ExistingColumn column, SchemaField field)
		{
			return false;
		}

		private bool AlterStringColumn(ExistingColumn column, SchemaField field)
		{
			return false;
		}

		private bool AlterNumberColumn(ExistingColumn column, SchemaField field)
		{
			return false;
		}

		internal void Drop()
		{
			try
			{
				new NodeAdminWriter(_node, string.Format(Strings.DropProcedure, TableName), CommandType.Text).Execute();
				new NodeAdminWriter(_node, string.Format(Strings.DropProcedureUpdate, PartialTableName), CommandType.Text).Execute();
				new NodeAdminWriter(_node, string.Format(Strings.DropStruct, TableName), CommandType.Text).Execute();
				new NodeAdminWriter(_node, string.Format(Strings.DropTable, TableName), CommandType.Text).Execute();
			}
			catch (Exception ex)
			{
				Log.Error(this, ex, LogEvents.DhDropPartitionFileError, _node.Name, _file.FileId.AsString());
			}
		}

		private bool AlterCoolColumn(ExistingColumn column, SchemaField field)
		{
			return false;
		}

		private bool AlterDateColumn(ExistingColumn column, SchemaField field)
		{
			return false;
		}

		private string ParseColumnMetaData(SchemaField field)
		{
			if (field is SchemaStringField)
				return ParseStringColumnMetaData(field as SchemaStringField);
			else if (field is SchemaNumberField)
				return ParseNumberColumnMetaData(field as SchemaNumberField);
			else if (field is SchemaBoolField)
				return ParseBoolColumnMetaData(field as SchemaBoolField);
			else if (field is SchemaDateField)
				return ParseDateColumnMetaData(field as SchemaDateField);
			else
				throw new NotSupportedException();
		}

		private string ParseStringColumnMetaData(SchemaStringField field)
		{
			return string.Format("NVARCHAR({0}) NULL", field.MaxLength);
		}

		private string ParseNumberColumnMetaData(SchemaNumberField field)
		{
			switch (field.NumberType)
			{
				case NumberFieldType.Byte:
					return "tinyint NULL";
				case NumberFieldType.Short:
					return "smallint NULL";
				case NumberFieldType.Int:
					return "int NULL";
				case NumberFieldType.Long:
					return "bigint NULL";
				case NumberFieldType.Float:
					return "float NULL";
				default:
					throw new NotSupportedException();
			}
		}

		private string ParseBoolColumnMetaData(SchemaBoolField field)
		{
			return "bit NULL";
		}

		private string ParseDateColumnMetaData(SchemaDateField field)
		{
			switch (field.DateType)
			{
				case DateFieldType.Date:
					return "date NULL";
				case DateFieldType.Time:
					return "time NULL";
				case DateFieldType.DateTime:
					return "datetime2(7) NULL";
				default:
					throw new NotSupportedException();
			}
		}

		private void RecreateHelpers()
		{
			new NodeAdminWriter(_node, string.Format(Strings.DropProcedure, TableName), CommandType.Text).Execute();
			new NodeAdminWriter(_node, string.Format(Strings.DropProcedureUpdate, PartialTableName), CommandType.Text).Execute();
			new NodeAdminWriter(_node, string.Format(Strings.DropStruct, TableName), CommandType.Text).Execute();

			new NodeAdminWriter(_node, string.Format(Strings.CreateTableType, TableName, CreateTableTypeColumns()), CommandType.Text).Execute();

			var commandText = string.Format(Strings.CreateMergeProcedure, TableName, CreateProcedureColumns(), CreateMergeOn(), CreateSetStatement(), CreateInsertFields());

			new NodeAdminWriter(_node, commandText, CommandType.Text).Execute();

			commandText = string.Format(Strings.CreateUpdateProcedure, PartialTableName, TableName, CreateProcedureColumns(), CreateMergeOn(), CreateSetStatement(), CreateInsertFields());

			new NodeAdminWriter(_node, commandText, CommandType.Text).Execute();
		}

		private string CreateProcedureColumns()
		{
			var sb = new StringBuilder();

			sb.Append("timestamp,");

			foreach (var i in Partition.Schema)
				sb.AppendFormat("{0},", i.Name);

			return sb.ToString().TrimEnd(',');
		}

		private string CreateTableTypeColumns()
		{
			var sb = new StringBuilder();

			sb.Append("timestamp datetime2 NOT NULL,");

			foreach (var i in Partition.Schema)
				sb.AppendFormat("{0} {1},", i.Name, GetSqlType(i));

			return sb.ToString().TrimEnd(',');
		}

		private string CreateMergeOn()
		{
			var sb = new StringBuilder();

			sb.Append("t.timestamp = s.timestamp");

			foreach (var i in Partition.Schema)
			{
				if (i.IsKey)
					sb.AppendFormat(" AND t.{0} = s.{0}", i.Name);
			}

			return sb.ToString();
		}

		private string CreateSetStatement()
		{
			var sb = new StringBuilder();

			foreach (var i in Partition.Schema)
			{
				if (i is SchemaNumberField)
				{
					if (((SchemaNumberField)i).Storage == NumberStorageMode.Sum)
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

			sb.AppendFormat("s.timestamp,");

			foreach (var i in Partition.Schema)
				sb.AppendFormat("s.{0},", i.Name);

			return sb.ToString().TrimEnd(',');
		}

		private string GetSqlType(SchemaField field)
		{
			if (field is SchemaStringField)
			{
				var sf = field as SchemaStringField;

				return string.Format("nvarchar ({0})", sf.MaxLength);
			}
			else if (field is SchemaNumberField)
			{
				var nt = field as SchemaNumberField;

				switch (nt.NumberType)
				{
					case NumberFieldType.Byte:
						return "tinyint";
					case NumberFieldType.Short:
						return "smallint";
					case NumberFieldType.Int:
						return "int";
					case NumberFieldType.Long:
						return "bigint";
					case NumberFieldType.Float:
						return "float";
					default:
						throw new NotSupportedException();
				}

			}
			else if (field is SchemaBoolField)
			{
				return "bit";
			}
			else if (field is SchemaDateField)
			{
				var dt = field as SchemaDateField;

				switch (dt.DateType)
				{
					case DateFieldType.Date:
						return "date";
					case DateFieldType.Time:
						return "time";
					case DateFieldType.DateTime:
						return "datetime2";
					default:
						throw new NotSupportedException();
				}
			}
			else
				throw new NotSupportedException();
		}
	}
}