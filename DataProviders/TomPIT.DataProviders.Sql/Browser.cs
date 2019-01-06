using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Sql
{
	internal class Browser : ISchemaBrowser
	{
		private const string Tables = "'U'";
		public const string Views = "'V'";
		public const string Procedures = "'P'";

		private string ResolveSchemaGroup(string schemaGroup)
		{
			if (string.Compare(schemaGroup, "Tables", true) == 0)
				return Tables;
			else if (string.Compare(schemaGroup, "Views", true) == 0)
				return Views;
			else if (string.Compare(schemaGroup, "Stored procedures", true) == 0)
				return Procedures;
			else
				throw new NotSupportedException();
		}

		public List<string> QueryGroupObjects(IConnection connection, string schemaGroup)
		{
			using (var c = new SqlConnection(connection.Value))
			{
				var command = new SqlCommand(string.Format("select o.object_id, o.name, o.type, s.name from sys.objects o inner join sys.schemas s on o.schema_id = s.schema_id where type in ({0})", ResolveSchemaGroup(schemaGroup)), c);
				var results = new List<string>();
				SqlDataReader rdr = null;

				try
				{

					c.Open();
					rdr = command.ExecuteReader();

					while (rdr.Read())
					{
						string name = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
						string schema = rdr.IsDBNull(3) ? string.Empty : string.Format("[{0}].", rdr.GetString(3));

						results.Add(string.Format("{0}{1}", schema, string.Format("[{0}]", name)));
					}

					results.Sort();

					return results;
				}
				finally
				{
					if (rdr != null && !rdr.IsClosed)
						rdr.Close();

					if (c != null && c.State != ConnectionState.Closed)
						c.Close();
				}
			}
		}

		public List<string> QuerySchemaGroups(IConnection connection)
		{
			List<string> r = new List<string>
			{
				"Tables",
				"Views",
				"Stored procedures"
			};

			return r;
		}

		public List<ISchemaField> QuerySchema(IConnection connection, string schemaGroup, string groupObject)
		{
			string sg = ResolveSchemaGroup(schemaGroup);

			if (string.Compare(sg, Tables, true) == 0
				|| string.Compare(sg, Views, true) == 0)
				return QueryTableSchema(connection, groupObject);
			else if (string.Compare(sg, Procedures, true) == 0)
				return QueryProcedureSchema(connection, groupObject);
			else
				throw new NotSupportedException();
		}

		public List<ISchemaParameter> QueryParameters(IConnection connection, string schemaGroup, string groupObject)
		{
			string sg = ResolveSchemaGroup(schemaGroup);

			if (string.Compare(sg, Procedures, true) == 0)
				return QueryProcedureParameters(connection, groupObject);
			else
				return new List<ISchemaParameter>();
		}

		private List<ISchemaParameter> QueryProcedureParameters(IConnection connection, string groupObject)
		{
			using (SqlConnection con = new SqlConnection(connection.Value))
			{
				SqlCommand command = new System.Data.SqlClient.SqlCommand(groupObject, con);

				try
				{
					command.CommandType = CommandType.StoredProcedure;
					con.Open();

					SqlCommandBuilder.DeriveParameters(command);

					List<ISchemaParameter> results = new List<ISchemaParameter>();

					foreach (SqlParameter p in command.Parameters)
					{
						if (string.Compare(p.ParameterName, "@return_value", true) == 0)
							continue;

						if (p.Direction != ParameterDirection.Input && p.Direction != ParameterDirection.InputOutput)
							continue;

						var item = new SchemaParameter
						{
							Name = p.ParameterName,
							DataType = Types.ToDataType(p.DbType),
							IsNullable = p.IsNullable
						};

						results.Add(item);
					}

					return results;
				}
				finally
				{
					if (con != null && con.State != ConnectionState.Closed)
						con.Close();
				}
			}
		}

		private List<ISchemaField> QueryTableSchema(IConnection connection, string groupObject)
		{
			using (var con = new SqlConnection(connection.Value))
			{
				var com = new SqlCommand(string.Format("SELECT * FROM {0}", groupObject), con);
				SqlDataReader rdr = null;

				try
				{
					con.Open();

					rdr = com.ExecuteReader(CommandBehavior.SchemaOnly);

					return ExtractFields(rdr.GetSchemaTable());
				}
				finally
				{
					if (rdr != null && !rdr.IsClosed)
						rdr.Close();

					if (con.State == ConnectionState.Open)
						con.Close();
				}
			}
		}

		private List<ISchemaField> QueryProcedureSchema(IConnection connection, string groupObject)
		{
			using (var con = new SqlConnection(connection.Value))
			{
				var com = new SqlCommand(groupObject, con);

				com.CommandType = CommandType.StoredProcedure;

				SqlDataReader rdr = null;

				try
				{
					con.Open();
					SqlCommandBuilder.DeriveParameters(com);

					rdr = com.ExecuteReader(CommandBehavior.SchemaOnly);

					return ExtractFields(rdr.GetSchemaTable());
				}
				finally
				{
					if (rdr != null && !rdr.IsClosed)
						rdr.Close();

					if (con.State == ConnectionState.Open)
						con.Close();
				}
			}

		}

		private static List<ISchemaField> ExtractFields(DataTable schema)
		{
			var items = new List<ISchemaField>();

			foreach (DataRow row in schema.Rows)
			{
				var item = new SchemaField
				{
					Name = row["ColumnName"].ToString(),
					DataType = Types.ToDataType(row["DataType"] as Type)
				};

				items.Add(item);
			}

			return items;
		}

		public ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject)
		{
			var r = new CommandDescriptor();

			string group = ResolveSchemaGroup(schemaGroup);

			if (string.Compare(group, Tables, true) == 0
				|| string.Compare(group, Views, true) == 0)
			{
				r.CommandType = CommandType.Text;
				r.CommandText = string.Format("SELECT * FROM {0}", groupObject);
			}
			else if (string.Compare(group, Procedures, true) == 0)
			{
				r.CommandType = CommandType.StoredProcedure;
				r.CommandText = groupObject;
			}
			else
				throw new NotSupportedException();

			return r;
		}
	}
}
