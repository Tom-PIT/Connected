using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class TableSynchronizer : SynchronizerBase
	{
		private List<IndexDescriptor> _indexes = null;
		private List<string> _usedConstraintNames = null;
		private ExistingModelSchema _existingSchema = null;
		public TableSynchronizer(SqlCommand command, IModelSchema schema) : base(command, schema)
		{
		}

		protected override void OnExecute()
		{
			if (TableExists())
				AlterTable();
			else
				CreateTable();
		}

		private bool TableExists()
		{
			Command.CommandText = $"IF (EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = '{Undress(SchemaName)}'  AND  TABLE_NAME = '{Undress(Schema.Name)}')) SELECT 1 ELSE SELECT 0";

			return Types.Convert<bool>(Command.ExecuteScalar());
		}

		private void CreateTable()
		{
			var text = new StringBuilder();

			text.AppendLine($"CREATE TABLE {Qualifier(SchemaName, Schema.Name)}");
			text.AppendLine("(");

			var primaryKey = Schema.Columns.FirstOrDefault(f => f.IsPrimaryKey);
			var unique = Schema.Columns.Where(f => f.IsUnique).ToList();

			for (var i = 0; i < Schema.Columns.Count; i++)
			{
				var column = Schema.Columns[i];

				text.Append(CreateColumnCommandText(column));

				if (i < Schema.Columns.Count - 1 || (primaryKey != null || unique.Count > 0))
					text.AppendLine(",");
			}

			if (primaryKey != null)
			{
				text.AppendLine($"CONSTRAINT [pk_{primaryKey.Name}] PRIMARY KEY CLUSTERED");
				text.AppendLine("(");
				text.AppendLine($"[{primaryKey.Name}] ASC");
				text.AppendLine(")");
			}

			text.AppendLine(");");

			Command.CommandText = text.ToString();
			Command.ExecuteNonQuery();

			CreateDefaults();
			CreateIndexes();
		}

		private void CreateIndexes()
		{
			foreach (var index in Indexes)
				CreateIndex(index);
		}

		private List<IndexDescriptor> Indexes
		{
			get
			{
				if (_indexes == null)
					_indexes = ParseIndexes(Schema);

				return _indexes;
			}
		}

		private List<IndexDescriptor> ParseIndexes(IModelSchema schema)
		{
			var result = new List<IndexDescriptor>();

			foreach (var column in schema.Columns)
			{
				if (column.IsIndex)
				{
					if (string.IsNullOrWhiteSpace(column.IndexGroup))
					{
						var index = new IndexDescriptor
						{
							Unique = column.IsUnique,
						};

						index.Columns.Add(column.Name);

						result.Add(index);
					}
					else
					{
						var index = result.FirstOrDefault(f => string.Compare(f.Group, column.IndexGroup, true) == 0);

						if (index == null)
						{
							index = new IndexDescriptor
							{
								Group = column.IndexGroup,
								Unique = column.IsUnique
							};

							result.Add(index);
						}

						index.Columns.Add(column.Name);
					}
				}
			}

			return result;
		}

		private void CreateDefaults()
		{
			foreach (var column in Schema.Columns)
			{
				if (!string.IsNullOrWhiteSpace(column.DefaultValue))
					AddDefault(column.Name, column.DefaultValue);
			}
		}

		private void AlterTable()
		{
			var existingIndexes = ParseIndexes(ExistingSchema);

			foreach (var existingColumn in ExistingSchema.Columns)
			{
				var column = Schema.Columns.FirstOrDefault(f => string.Compare(f.Name, existingColumn.Name, true) == 0);

				if (column == null)
					DropColumn(existingColumn, ExistingSchema, existingIndexes);
				else
					AlterColumn(existingColumn, ExistingSchema, existingIndexes, column);
			}

			foreach (var column in Schema.Columns)
			{
				var existing = ExistingSchema.Columns.FirstOrDefault(f => string.Compare(f.Name, column.Name, true) == 0);

				if (existing == null)
					AddColumn(column);
			}
		}

		private void AddColumn(IModelSchemaColumn column)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"ALTER TABLE {Qualifier(SchemaName, Schema.Name)} ADD COLUMN {CreateColumnCommandText(column)}");

			Command.CommandText = sb.ToString();
			Command.CommandType = CommandType.Text;
			Command.Parameters.Clear();

			Command.ExecuteNonQuery();

			if (column.IsPrimaryKey)
				AddPrimaryKey(column);

			if (!string.IsNullOrWhiteSpace(column.DefaultValue))
				AddDefault(column.Name, column.DefaultValue);
		}

		private void AddPrimaryKey(IModelSchemaColumn column)
		{
			Command.CommandText = $"ALTER TABLE {CreateColumnCommandText(column)} ADD CONSTRAINT {GenerateIndexName()} PRIMARY KEY CLUSTERED ({Qualifier(column.Name)}) ON PRIMARY";
			Command.CommandType = CommandType.Text;
			Command.Parameters.Clear();

			Command.ExecuteNonQuery();
		}

		private void AlterColumn(IModelSchemaColumn existingColumn, IModelSchema existingSchema, List<IndexDescriptor> existingIndexes, IModelSchemaColumn column)
		{
			if (existingColumn.Equals(column))
				return;
		}

		private void DropColumn(IModelSchemaColumn existingColumn, IModelSchema existingSchema, List<IndexDescriptor> existingIndexes)
		{
			if (!string.IsNullOrWhiteSpace(existingColumn.DefaultValue))
				DropDefault(existingColumn);

			var indexes = ExistingSchema.ResolveIndexes(existingColumn.Name);

			foreach (var index in indexes)
			{
				if (index.IsConstraint)
					DropConstraint(index);
				else
					DropIndex(index);
			}

			Command.CommandText = $"ALTER TABLE {Qualifier(SchemaName, Schema.Name)} DROP COLUMN {existingColumn.Name};";
			Command.CommandType = CommandType.Text;
			Command.Parameters.Clear();

			Command.ExecuteNonQuery();
		}
		private void DropConstraint(ExistingIndex index)
		{
			if (ExistingSchema.ExistingIndexes.FirstOrDefault(f => string.Compare(f.Name, index.Name, true) == 0) == null)
				return;

			Command.CommandText = $"ALTER TABLE {Qualifier(SchemaName, Schema.Name)} DROP CONSTRAINT {index.Name};";
			Command.CommandType = CommandType.Text;
			Command.Parameters.Clear();

			Command.ExecuteNonQuery();

			ExistingSchema.ExistingIndexes.Remove(index);
		}

		private void DropIndex(ExistingIndex index)
		{
			if (ExistingSchema.ExistingIndexes.FirstOrDefault(f => string.Compare(f.Name, index.Name, true) == 0) == null)
				return;

			Command.CommandText = $"DROP INDEX {index.Name} ON {Qualifier(SchemaName, Schema.Name)};";
			Command.CommandType = CommandType.Text;
			Command.Parameters.Clear();

			Command.ExecuteNonQuery();

			ExistingSchema.ExistingIndexes.Remove(index);
		}

		private string CreateColumnCommandText(IModelSchemaColumn column)
		{
			var builder = new StringBuilder();

			builder.AppendFormat($"{Qualifier(column.Name)} {CreateDataTypeMetaData(column)} ");

			if (column.IsIdentity)
				builder.Append("IDENTITY(1,1) ");

			if (column.IsNullable)
				builder.Append("NULL ");
			else
				builder.Append("NOT NULL ");

			return builder.ToString();
		}

		private void AddDefault(string column, string value)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"ALTER TABLE {Qualifier(SchemaName, Schema.Name)} ADD CONSTRAINT DF_{Undress(SchemaName)}_{Undress(Schema.Name)}_{column} DEFAULT {value} FOR {column}");

			Command.CommandText = sb.ToString();
			Command.ExecuteNonQuery();
		}

		private void DropDefault(IModelSchemaColumn column)
		{
			Command.CommandText = $"ALTER TABLE {Qualifier(SchemaName, Schema.Name)} DROP CONSTRAINT DF_{Undress(SchemaName)}_{Undress(Schema.Name)}_{column};";
			Command.CommandType = CommandType.Text;
			Command.Parameters.Clear();

			Command.ExecuteNonQuery();
		}

		private void CreateIndex(IndexDescriptor index)
		{
			if (index.Unique)
				CreateUniqueConstraint(index);
			else
			{
				var sb = new StringBuilder();

				sb.AppendLine($"CREATE NONCLUSTERED INDEX [{GenerateIndexName()}] ON {Qualifier(SchemaName, Schema.Name)}(");
				var comma = string.Empty;

				foreach (var column in index.Columns)
				{
					sb.AppendLine($"{comma}{Qualifier(column)} ASC");

					comma = ",";
				}

				sb.AppendLine(") ON [PRIMARY]");

				Command.CommandText = sb.ToString();
				Command.ExecuteNonQuery();
			}
		}

		private void CreateUniqueConstraint(IndexDescriptor index)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"ALTER TABLE {Qualifier(SchemaName, Schema.Name)} ADD CONSTRAINT [{GenerateIndexName()}] UNIQUE NONCLUSTERED (");
			var comma = string.Empty;

			foreach (var column in index.Columns)
			{
				sb.AppendLine($"{comma}{Qualifier(column)} ASC");

				comma = ",";
			}

			sb.AppendLine(") ON [PRIMARY]");

			Command.CommandText = sb.ToString();
			Command.ExecuteNonQuery();
		}

		private ExistingModelSchema ExistingSchema
		{
			get
			{
				if (_existingSchema == null)
				{
					_existingSchema = new ExistingModelSchema();
					_existingSchema.Load(this, Command);
				}

				return _existingSchema;
			}
		}

		private List<string> UsedConstraintNames
		{
			get
			{
				if (_usedConstraintNames == null)
					_usedConstraintNames = new List<string>();

				return _usedConstraintNames;
			}
		}

		private string GenerateIndexName()
		{
			var index = 0;

			while (true)
			{
				var value = $"IX_{SchemaName.ToLowerInvariant()}_{Schema.Name}";

				if (index > 0)
					value = $"{value}_{index}";

				if (!UsedConstraintNames.Contains(value, StringComparer.OrdinalIgnoreCase))
				{
					UsedConstraintNames.Add(value);
					return value;
				}

				index++;
			}
		}
	}
}
