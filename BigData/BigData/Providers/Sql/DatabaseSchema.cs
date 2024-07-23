using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TomPIT.BigData.Data;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagnostics;
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
		}

		public string TableName { get { return string.Format("t_{0}", File.TableName()); } }
		public string PartialTableName { get { return string.Format("p_{0}", File.TableName()); } }

		private bool TableExists
		{
			get
			{
				using var reader = new NodeAdminScalarReader<int>(Node, string.Format(SqlStrings.TableExists, TableName), CommandType.Text);
				return reader.ExecuteScalar(0) == 1;
			}
		}

		private IPartitionConfiguration Partition => _partition ??= MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(File.Partition) as IPartitionConfiguration;

		private PartitionSchema Schema => _schema ??= new PartitionSchema(Partition);

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
					var action = i.Index ? SqlStrings.CreateIndex : SqlStrings.DropIndex;

					using var writer = new NodeAdminWriter(Node, string.Format(SqlStrings.CreateIndex, TableName, i.Name), CommandType.Text);
					writer.Execute();
				}
			}
		}

		private void CreateColumn(PartitionSchemaField field)
		{
			string fieldName = field.Name.ToLowerInvariant();

			using var columnCreateWriter = new NodeAdminWriter(Node, string.Format(SqlStrings.ColumnCreate, TableName, fieldName, ParseColumnMetaData(field)), System.Data.CommandType.Text);
			columnCreateWriter.Execute();

			if (field.Index)
			{
				using var createIndexWriter = new NodeAdminWriter(Node, string.Format(SqlStrings.CreateIndex, TableName, fieldName), CommandType.Text);
				createIndexWriter.Execute();
			}
		}

		private void CreateTable()
		{
			using var createWriter = new NodeAdminWriter(Node, string.Format(SqlStrings.TableCreate, TableName), CommandType.Text);
			createWriter.Execute();

			using var timestampWriter = new NodeAdminWriter(Node, string.Format(SqlStrings.TableAddTimestampDefault, TableName), CommandType.Text);
			timestampWriter.Execute();

			//create index on timestamp field
			using var indexWriter = new NodeAdminWriter(Node, string.Format(SqlStrings.CreateIndex, TableName, Merger.TimestampColumn), CommandType.Text);
			indexWriter.Execute();
		}

		private List<ExistingColumn> LoadExistingColumns()
		{
			using var reader = new NodeAdminReader<ExistingColumn>(Node, string.Format(SqlStrings.ColumnQuery, TableName), CommandType.Text);
			return reader.Execute();
		}

		private void DropColumn(ExistingColumn col)
		{
			using var writer = new NodeAdminWriter(Node, string.Format(SqlStrings.ColumnDrop, TableName, col.ColumnName.ToLowerInvariant()), CommandType.Text);
			writer.Execute();
		}

		internal void Drop()
		{
			try
			{
				using var writer = new NodeAdminWriter(Node, string.Format(SqlStrings.DropTable, TableName), CommandType.Text);
				writer.Execute();
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

		private bool IsSystemColumn(string name)
		{
			return string.Compare(name, Merger.TimestampColumn, true) == 0
				|| string.Compare(name, Merger.IdColumn, true) == 0;
		}
	}
}