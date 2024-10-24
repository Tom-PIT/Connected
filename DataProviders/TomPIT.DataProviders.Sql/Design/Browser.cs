﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;

namespace TomPIT.DataProviders.Sql.Design
{
	internal class Browser : SchemaBrowser
	{
		private const string Tables = "'U'";
		private const string Views = "'V'";
		private const string Procedures = "'P'";

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

		public override List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration)
		{
			return QueryGroupObjects(configuration, "Stored procedures");
		}
		public override List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration, string schemaGroup)
		{
			using var c = new SqlConnection(ResolveConnectionString(configuration).Value);
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			var command = new SqlCommand(string.Format("select o.object_id, o.name, o.type, s.name from sys.objects o inner join sys.schemas s on o.schema_id = s.schema_id where type in ({0})", ResolveSchemaGroup(schemaGroup)), c);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
			var results = new List<IGroupObject>();
			SqlDataReader rdr = null;

			try
			{

				c.Open();
				rdr = command.ExecuteReader();

				while (rdr.Read())
				{
					var name = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
					var schema = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);

					results.Add(new GroupObject
					{
						Text = $"{name} ({schema})",
						Value = $"[{schema}].[{name}]"
					});
				}

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

		public override List<string> QuerySchemaGroups(IConnectionConfiguration configuration)
		{
			var r = new List<string>
			{
				"Tables",
				"Views",
				"Stored procedures"
			};

			return r;
		}

		public override List<ISchemaField> QuerySchema(IConnectionConfiguration configuration, string schemaGroup, string groupObject)
		{
			var sg = ResolveSchemaGroup(schemaGroup);

			if (string.Compare(sg, Tables, true) == 0
				|| string.Compare(sg, Views, true) == 0)
				return QueryTableSchema(configuration, groupObject);
			else if (string.Compare(sg, Procedures, true) == 0)
				return QueryProcedureSchema(configuration, groupObject);
			else
				throw new NotSupportedException();
		}

		public override List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string schemaGroup, string groupObject, DataOperation operation)
		{
			var sg = ResolveSchemaGroup(schemaGroup);

			if (string.Compare(sg, Procedures, true) == 0)
				return QueryProcedureParameters(configuration, groupObject);
			else
				return new List<ISchemaParameter>();
		}

		private List<ISchemaParameter> QueryProcedureParameters(IConnectionConfiguration configuration, string groupObject)
		{
			using var con = new SqlConnection(ResolveConnectionString(configuration).Value);
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			var command = new SqlCommand(groupObject, con);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

			try
			{
				command.CommandType = CommandType.StoredProcedure;
				con.Open();

				SqlCommandBuilder.DeriveParameters(command);

				var results = new List<ISchemaParameter>();

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

				ResolveNullableColumns(con, groupObject, results);

				return results;
			}
			finally
			{
				if (con != null && con.State != ConnectionState.Closed)
					con.Close();
			}
		}

		private void ResolveNullableColumns(SqlConnection con, string procedure, List<ISchemaParameter> results)
		{
			var com = new SqlCommand("sp_helptext", con)
			{
				CommandType = CommandType.StoredProcedure
			};

			com.Parameters.AddWithValue("@objname", procedure);

			var r = com.ExecuteReader();
			var declarations = new List<string>();
			var sb = new StringBuilder();

			while (r.Read())
				sb.AppendFormat(" {0} ", r.GetString(0).Trim());

			r.Close();

			var definition = Regex.Replace(sb.ToString(), @"\t|\n|\r", " ");
			var header = definition.ToString().Substring(0, definition.IndexOf(" AS "));

			if (!header.Contains("@"))
				return;

			var parametersSection = header.Substring(header.IndexOf("@"));
			var tokens = parametersSection.Split(',');

			foreach (var i in tokens)
			{
				if (!i.Contains('='))
					continue;

				var parameterTokens = i.Trim().Split(new char[] { ' ' }, 2);
				var parameter = results.FirstOrDefault(f => string.Compare(f.Name, parameterTokens[0].Trim(), true) == 0);

				if (parameter == null)
					continue;

				((SchemaParameter)parameter).IsNullable = true;
			}
		}

		private List<ISchemaField> QueryTableSchema(IConnectionConfiguration configuration, string groupObject)
		{
			using var con = new SqlConnection(ResolveConnectionString(configuration).Value);
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			var com = new SqlCommand(string.Format("SELECT * FROM {0}", groupObject), con);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
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

		private List<ISchemaField> QueryProcedureSchema(IConnectionConfiguration configuration, string groupObject)
		{
			using var con = new SqlConnection(ResolveConnectionString(configuration).Value);
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			var com = new SqlCommand(groupObject, con);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

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

		public override ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject)
		{
			var r = new CommandDescriptor();
			var group = ResolveSchemaGroup(schemaGroup);

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

		public override List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string groupObject, DataOperation operation)
		{
			return QueryParameters(configuration, "Stored procedures", groupObject, DataOperation.NotSet);
		}
	}
}
