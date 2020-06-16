using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ExistingModelSchema : IModelSchema
	{
		private List<IModelSchemaColumn> _columns = null;
		private List<ExistingIndex> _existingIndexes = null;
		public List<IModelSchemaColumn> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<IModelSchemaColumn>();

				return _columns;
			}
		}

		public string Schema { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public bool Ignore { get; set; }

		public string Dependency { get; set; }

		public void Load(TableSynchronizer owner, SqlCommand command)
		{
			command.CommandText = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{owner.SchemaName}' AND TABLE_NAME = '{owner.Schema.Name}'";
			command.Parameters.Clear();
			var rdr = command.ExecuteReader();

			while (rdr.Read())
			{
				var column = new ExistingSchemaColumn
				{
					IsNullable = string.Compare(owner.GetValue(rdr, "IS_NULLABLE", string.Empty), "NO", true) == 0 ? false : true,
					DataType = owner.ToDbType(owner.GetValue(rdr, "DATA_TYPE", string.Empty)),
					MaxLength = owner.GetValue(rdr, "CHARACTER_MAXIMUM_LENGTH", 0),
					Name = owner.GetValue(rdr, "COLUMN_NAME", string.Empty),
				};

				Columns.Add(column);
			}

			rdr.Close();

			command.CommandText = "sp_help";
			command.Parameters.Clear();
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue("@objname", owner.Qualifier(owner.SchemaName, owner.Schema.Name));
			rdr = command.ExecuteReader();

			rdr.NextResult();
			rdr.NextResult();

			if (rdr.Read())
			{
				var col = owner.GetValue(rdr, "Identity", string.Empty);

				if (Columns.FirstOrDefault(f => string.Compare(f.Name, col, true) == 0) is ExistingSchemaColumn c)
					c.IsIdentity = true;
			}

			rdr.NextResult();
			rdr.NextResult();
			rdr.NextResult();

			while (rdr.Read())
			{
				var indexName = owner.GetValue(rdr, "index_name", string.Empty);
				var indexDescription = owner.GetValue(rdr, "index_description", string.Empty);
				var indexKeys = owner.GetValue(rdr, "index_keys", string.Empty);

				ExistingIndexes.Add(new ExistingIndex
				{
					Description = indexDescription,
					Keys = indexKeys,
					Name = indexName
				});

				var keys = indexKeys.Split(',');
				var descriptionTokens = indexDescription.Split(',');
				var unique = false;
				var primaryKey = true;

				foreach (var descriptionToken in descriptionTokens)
				{
					if (string.Compare(descriptionToken.Trim(), "unique", true) == 0)
						unique = true;
					else if (descriptionToken.Trim().StartsWith("primary key "))
						primaryKey = true;
				}

				foreach (var key in keys)
				{
					var column = key.Trim();

					if (Columns.FirstOrDefault(f => string.Compare(f.Name, column, true) == 0) is ExistingSchemaColumn existingColumn)
					{
						existingColumn.IsIndex = true;
						existingColumn.IsUnique = unique;
						existingColumn.IsPrimaryKey = primaryKey;
					}
				}
			}

			rdr.NextResult();

			while (rdr.Read())
			{
				var type = owner.GetValue(rdr, "constraint_type", string.Empty);

				if (type.ToLowerInvariant().StartsWith("default "))
				{
					var column = type.Split(' ')[^1];

					if (Columns.FirstOrDefault(f => string.Compare(f.Name, column, true) == 0) is ExistingSchemaColumn existingColumn)
					{
						var value = owner.GetValue(rdr, "constraint_keys", string.Empty);

						if (value.StartsWith("(") && value.EndsWith(")"))
							value = value[1..^1];

						existingColumn.DefaultValue = value;
					}
				}
			}

			rdr.Close();
			command.Parameters.Clear();
			command.CommandType = CommandType.Text;
		}

		public List<ExistingIndex> ExistingIndexes
		{
			get
			{
				if (_existingIndexes == null)
					_existingIndexes = new List<ExistingIndex>();

				return _existingIndexes;
			}
		}

		public List<ExistingIndex> ResolveIndexes(string column)
		{
			var result = new List<ExistingIndex>();

			foreach (var index in ExistingIndexes)
			{
				if (index.IsReferencedBy(column))
					result.Add(index);
			}

			return result;
		}
	}
}
