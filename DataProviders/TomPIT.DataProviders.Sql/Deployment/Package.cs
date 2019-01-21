using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TomPIT.Data.DataProviders.Deployment;

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

				var type = rdr.GetString(rdr.GetOrdinal("TABLE_TYPE"));

				if (string.Compare(type, "BASE_TABLE", true) != 0)
					continue;

				t.Schema = rdr.GetString(rdr.GetOrdinal("TABLE_SCHEMA"));
				t.Name = rdr.GetString(rdr.GetOrdinal("TABLE_NAME"));

				db.Tables.Add(t);
			}

			rdr.Close();

			com.CommandText = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS";
			rdr = com.ExecuteReader();

			while (rdr.Read())
			{
				var schema = rdr.GetString(rdr.GetOrdinal("TABLE_SCHEMA"));
				var table = rdr.GetString(rdr.GetOrdinal("TABLE_NAME"));
				var t = FindTable(db, schema, table);

				if (t == null)
					continue;

				var c = new Column
				{
					CharacterMaximumLength = rdr.GetInt32(rdr.GetOrdinal("CHARACTER_MAXIMUM_LENGTH")),
					CharacterOctetLength = rdr.GetInt32(rdr.GetOrdinal("CHARACTER_OCTET_LENGTH")),
					CharacterSetName = rdr.GetString(rdr.GetOrdinal("CHARACTER_SET_NAME")),
					DataType = rdr.GetString(rdr.GetOrdinal("DATA_TYPE")),
					DateTimePrecision = rdr.GetInt16(rdr.GetOrdinal("DATETIME_PRECISION")),
					DefaultValue = rdr.GetString(rdr.GetOrdinal("COLUMN_DEFAULT")),
					IsNullable = string.Compare(rdr.GetString(rdr.GetOrdinal("IS_NULLABLE")), "NO", true) == 0 ? false : true,
					Name = rdr.GetString(rdr.GetOrdinal("COLUMN_NAME")),
					NumericPrecision = rdr.GetByte(rdr.GetOrdinal("NUMERIC_PRECISION")),
					NumericPrecisionRadix = rdr.GetInt16(rdr.GetOrdinal("NUMERIC_PRECISION_RADIX")),
					NumericScale = rdr.GetInt32(rdr.GetOrdinal("NUMERIC_SCALE")),
					Ordinal = rdr.GetInt32(rdr.GetOrdinal("ORDINAL_POSITION"))
				};

				t.Columns.Add(c);
			}

			rdr.Close();


			foreach (var i in db.Tables)
			{
				var cmd = new SqlCommand("sp_help", con);

				cmd.Parameters.AddWithValue("@objname", string.Format("{0}.{1}", i.Schema, i.Name));
				var r = cmd.ExecuteReader();

				r.NextResult();
				r.NextResult();

				if (r.Read())
				{
					var col = r.GetString(rdr.GetOrdinal("Identity"));
					var c = i.Columns.FirstOrDefault(f => string.Compare(f.Name, col, true) == 0) as Column;

					c.Identity = true;
				}

				r.Close();

				cmd = new SqlCommand("sp_helpindex", con);

				cmd.Parameters.AddWithValue("@objname", string.Format("{0}.{1}", i.Schema, i.Name));
				r = cmd.ExecuteReader();

				while (r.Read())
				{
					var idx = new TableIndex
					{
						Name = r.GetString(rdr.GetOrdinal("index_name"))
					};

					var cols = r.GetString(rdr.GetOrdinal("index_keys")).Split(',');

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
					var usage = columnsUsage.FirstOrDefault(f => string.Compare(f.ConstraintName, i.ConstraintName, true) == 0
						&& string.Compare(f.ConstraintSchema, i.ConstraintSchema, true) == 0);

					var col = t.Columns.FirstOrDefault(f => string.Compare(f.Name, usage.ColumnName, true) == 0);

					col.Constraints.Add(new TableConstraint
					{
						Name = usage.ColumnName,
						Schema = usage.ConstraintSchema,
						Type = i.ConstraintType
					});
				}
			}
		}

		private static void CreateViews(SqlConnection con, Database r)
		{

		}

		private static void CreateRoutines(SqlConnection con, Database r)
		{

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
					ColumnName = r.GetString(r.GetOrdinal("COLUMN_NAME")),
					ConstraintName = r.GetString(r.GetOrdinal("CONSTRAINT_NAME")),
					ConstraintSchema = r.GetString(r.GetOrdinal("CONSTRAINT_SCHEMA")),
					TableName = r.GetString(r.GetOrdinal("TABLE_NAME")),
					TableSchema = r.GetString(r.GetOrdinal("TABLE_SCHEMA")),
				});
			}

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
					DeleteRule = r.GetString(r.GetOrdinal("DELETE_RULE")),
					ConstraintName = r.GetString(r.GetOrdinal("CONSTRAINT_NAME")),
					ConstraintSchema = r.GetString(r.GetOrdinal("CONSTRAINT_SCHEMA")),
					MatchOption = r.GetString(r.GetOrdinal("MATCH_OPTION")),
					UpdateRule = r.GetString(r.GetOrdinal("UPDATE_RULE")),
					UniqueConstraintName = r.GetString(r.GetOrdinal("UNIQUE_CONSTRAINT_NAME")),
					UniqueConstraintSchema = r.GetString(r.GetOrdinal("UNIQUE_CONSTRAINT_SCHEMA"))
				});
			}

			return items;
		}

		private static List<InformationSchemaTableConstraints> ReadTableConstraints(SqlConnection connection)
		{
			var com = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS", connection);
			var r = com.ExecuteReader();
			var items = new List<InformationSchemaTableConstraints>();

			while (r.Read())
			{
				items.Add(new InformationSchemaTableConstraints
				{
					ConstraintType = r.GetString(r.GetOrdinal("CONSTRAINT_TYPE")),
					ConstraintName = r.GetString(r.GetOrdinal("CONSTRAINT_NAME")),
					ConstraintSchema = r.GetString(r.GetOrdinal("CONSTRAINT_SCHEMA")),
					TableSchema = r.GetString(r.GetOrdinal("TABLE_SCHEMA")),
					TableName = r.GetString(r.GetOrdinal("TABLE_NAME"))
				});
			}

			return items;
		}
	}
}
