using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal static class Package
	{
		public static IDatabase Create(string connectionString)
		{
			var r = new Database();

			using (var con = new SqlConnection(connectionString))
			{
				con.Open();

				try
				{
					CreateTables(con, r);
					CreateViews(con, r);
					CreateRoutines(con, r);

				}
				finally
				{
					con.Close();
				}
			}

			return r;
		}

		private static void CreateTables(SqlConnection con, Database db)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES", con);
			var rdr = com.ExecuteReader();

			while (rdr.Read())
			{
				var t = new Table();

				var type = rdr.GetValue("TABLE_TYPE", string.Empty);

				if (string.Compare(type, "BASE TABLE", true) != 0)
					continue;

				t.Schema = rdr.GetValue("TABLE_SCHEMA", string.Empty);
				t.Name = rdr.GetValue("TABLE_NAME", string.Empty);

				db.Tables.Add(t);
			}

			rdr.Close();

			com.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS";
			rdr = com.ExecuteReader();

			while (rdr.Read())
			{
				var schema = rdr.GetValue("TABLE_SCHEMA", string.Empty);
				var table = rdr.GetValue("TABLE_NAME", string.Empty);
				var t = FindTable(db, schema, table);

				if (t == null)
					continue;

				var c = new Column
				{
					CharacterMaximumLength = rdr.GetValue("CHARACTER_MAXIMUM_LENGTH", 0),
					CharacterOctetLength = rdr.GetValue("CHARACTER_OCTET_LENGTH", 0),
					CharacterSetName = rdr.GetValue("CHARACTER_SET_NAME", string.Empty),
					DataType = rdr.GetValue("DATA_TYPE", string.Empty),
					DateTimePrecision = rdr.GetValue("DATETIME_PRECISION", 0),
					DefaultValue = rdr.GetValue("COLUMN_DEFAULT", string.Empty),
					IsNullable = string.Compare(rdr.GetValue("IS_NULLABLE", string.Empty), "NO", true) == 0 ? false : true,
					Name = rdr.GetValue("COLUMN_NAME", string.Empty),
					NumericPrecision = rdr.GetValue("NUMERIC_PRECISION", 0),
					NumericPrecisionRadix = rdr.GetValue("NUMERIC_PRECISION_RADIX", 0),
					NumericScale = rdr.GetValue("NUMERIC_SCALE", 0),
					Ordinal = rdr.GetValue("ORDINAL_POSITION", 0)
				};

				t.Columns.Add(c);
			}

			rdr.Close();


			foreach (var i in db.Tables)
			{
				var cmd = new SqlCommand("sp_help", con);

				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@objname", string.Format("{0}.{1}", i.Schema, i.Name));
				var r = cmd.ExecuteReader();

				r.NextResult();
				r.NextResult();

				if (r.Read())
				{
					var col = r.GetValue("Identity", string.Empty);
					var c = i.Columns.FirstOrDefault(f => string.Compare(f.Name, col, true) == 0) as Column;

					if (c != null)
						c.Identity = true;
				}

				r.Close();

				cmd = new SqlCommand("sp_helpindex", con);

				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@objname", string.Format("{0}.{1}", i.Schema, i.Name));
				r = cmd.ExecuteReader();

				while (r.Read())
				{
					var idx = new TableIndex
					{
						Name = r.GetValue("index_name", string.Empty)
					};

					var cols = r.GetValue("index_keys", string.Empty).Split(',');

					var description = r.GetValue("index_description", string.Empty);

					if (description.Contains("unique") || description.Contains("primary key"))
						continue;

					foreach (var j in cols)
					{
						if (string.IsNullOrWhiteSpace(j))
							continue;

						idx.Columns.Add(j.Trim());
					}

					i.Indexes.Add(idx);
				}

				r.Close();
			}

			var columnsUsage = ReadColumnsUsage(con);
			var refConstraints = ReadReferentialConstraints(con);
			var tableConstraints = ReadTableConstraints(con);

			foreach (var i in tableConstraints)
			{
				var t = FindTable(db, i.TableSchema, i.TableName);

				if (t == null)
					continue;

				if (string.Compare(i.ConstraintType, "FOREIGN KEY", true) == 0)
				{
					var reference = refConstraints.FirstOrDefault(f => string.Compare(f.ConstraintName, i.ConstraintName, true) == 0
						&& string.Compare(f.ConstraintSchema, i.ConstraintSchema, true) == 0);

					var usage = columnsUsage.FirstOrDefault(f => string.Compare(f.ConstraintName, i.ConstraintName, true) == 0
						&& string.Compare(f.ConstraintSchema, i.ConstraintSchema, true) == 0);

					var col = t.Columns.FirstOrDefault(f => string.Compare(f.Name, usage.ColumnName, true) == 0).Reference as ReferentialConstraint;

					col.MatchOption = reference.MatchOption;
					col.Name = reference.ConstraintName;
					col.ReferenceName = reference.UniqueConstraintName;
					col.ReferenceSchema = reference.UniqueConstraintSchema;
					col.UpdateRule = reference.UpdateRule;
				}
				else if (string.Compare(i.ConstraintType, "PRIMARY KEY", true) == 0
					|| string.Compare(i.ConstraintType, "UNIQUE", true) == 0)
				{
					var columns = columnsUsage.Where(f => string.Compare(f.ConstraintName, i.ConstraintName, true) == 0);

					foreach (var column in columns)
					{
						var col = t.Columns.FirstOrDefault(f => string.Compare(f.Name, column.ColumnName, true) == 0);

						col.Constraints.Add(new TableConstraint
						{
							Name = i.ConstraintName,
							Schema = i.ConstraintSchema,
							Type = i.ConstraintType
						});
					}
				}
			}
		}

		private static void CreateViews(SqlConnection con, Database db)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.VIEWS", con);
			var r = com.ExecuteReader();

			while (r.Read())
			{
				db.Views.Add(new View
				{
					Definition = r.GetValue("VIEW_DEFINITION", string.Empty),
					Name = r.GetValue("TABLE_NAME", string.Empty),
					Schema = r.GetValue("TABLE_SCHEMA", string.Empty)
				});
			}

			r.Close();
		}

		private static void CreateRoutines(SqlConnection con, Database db)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.ROUTINES", con);
			var r = com.ExecuteReader();

			while (r.Read())
			{
				db.Routines.Add(new Routine
				{
					Definition = r.GetValue("ROUTINE_DEFINITION", string.Empty),
					Name = r.GetValue("ROUTINE_NAME", string.Empty),
					Schema = r.GetValue("ROUTINE_SCHEMA", string.Empty),
					Type = r.GetValue("ROUTINE_TYPE", string.Empty)
				});
			}

			r.Close();
		}

		private static ITable FindTable(IDatabase db, string schema, string name)
		{
			return db.Tables.FirstOrDefault(f => string.Compare(schema, f.Schema, true) == 0
				&& string.Compare(f.Name, name, true) == 0);
		}

		private static List<InformationSchemaColumnUsage> ReadColumnsUsage(SqlConnection connection)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE", connection);
			var r = com.ExecuteReader();
			var items = new List<InformationSchemaColumnUsage>();

			while (r.Read())
			{
				items.Add(new InformationSchemaColumnUsage
				{
					ColumnName = r.GetValue("COLUMN_NAME", string.Empty),
					ConstraintName = r.GetValue("CONSTRAINT_NAME", string.Empty),
					ConstraintSchema = r.GetValue("CONSTRAINT_SCHEMA", string.Empty),
					TableName = r.GetValue("TABLE_NAME", string.Empty),
					TableSchema = r.GetValue("TABLE_SCHEMA", string.Empty),
				});
			}

			r.Close();

			return items;
		}

		private static List<InformationSchemaReferentialConstraint> ReadReferentialConstraints(SqlConnection connection)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS", connection);
			var r = com.ExecuteReader();
			var items = new List<InformationSchemaReferentialConstraint>();

			while (r.Read())
			{
				items.Add(new InformationSchemaReferentialConstraint
				{
					DeleteRule = r.GetValue("DELETE_RULE", string.Empty),
					ConstraintName = r.GetValue("CONSTRAINT_NAME", string.Empty),
					ConstraintSchema = r.GetValue("CONSTRAINT_SCHEMA", string.Empty),
					MatchOption = r.GetValue("MATCH_OPTION", string.Empty),
					UpdateRule = r.GetValue("UPDATE_RULE", string.Empty),
					UniqueConstraintName = r.GetValue("UNIQUE_CONSTRAINT_NAME", string.Empty),
					UniqueConstraintSchema = r.GetValue("UNIQUE_CONSTRAINT_SCHEMA", string.Empty)
				});
			}

			r.Close();

			return items;
		}

		private static List<InformationSchemaTableConstraints> ReadTableConstraints(SqlConnection connection)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS", connection);
			var r = com.ExecuteReader();
			var items = new List<InformationSchemaTableConstraints>();

			while (r.Read())
			{
				items.Add(new InformationSchemaTableConstraints
				{
					ConstraintName = r.GetValue("CONSTRAINT_NAME", string.Empty),
					ConstraintSchema = r.GetValue("CONSTRAINT_SCHEMA", string.Empty),
					TableSchema = r.GetValue("TABLE_SCHEMA", string.Empty),
					TableName = r.GetValue("TABLE_NAME", string.Empty),
					ConstraintType = r.GetValue("CONSTRAINT_TYPE", string.Empty)
				});
			}

			r.Close();

			return items;
		}

		private static T GetValue<T>(this SqlDataReader r, string fieldName, T defaultValue)
		{
			var idx = r.GetOrdinal(fieldName);

			if (idx == -1)
				return defaultValue;

			if (r.IsDBNull(idx))
				return defaultValue;

			return (T)Convert.ChangeType(r.GetValue(idx), typeof(T));
		}
	}
}
