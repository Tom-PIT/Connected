using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using TomPIT.Data.DataProviders;
using TomPIT.Data.Sql;
using TomPIT.DataProviders.Sql.Deployment;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql
{
	[SchemaBrowser("TomPIT.DataProviders.Sql.Browser, TomPIT.DataProviders.Sql")]
	public class SqlDataProvider : IDataProvider
	{
		public Guid Id => new Guid("{C5849300-11A4-4FAE-B433-3C89DD05DDF0}");

		public string Name => "Microsoft SQL Server";

		public bool SupportsDeploy => true;

		public void Execute(IDataCommandDescriptor command)
		{
			Execute(command, null);
		}

		public void Execute(IDataCommandDescriptor command, IDataConnection connection)
		{
			var con = ResolveConnection(command, connection);
			var com = ResolveCommand(command, con, connection);

			Execute(command, con, com, connection != null);
		}

		private void Execute(IDataCommandDescriptor command, ReliableSqlConnection connection, SqlCommand cmd, bool externalConnection)
		{
			if (connection.State == ConnectionState.Closed)
				connection.Open();

			try
			{
				SetupParameters(command, cmd);

				foreach (var i in command.Parameters)
					cmd.Parameters[i.Name].Value = i.Value;

				cmd.ExecuteNonQuery();

				foreach (var i in command.Parameters)
				{
					if (i.Direction == ParameterDirection.ReturnValue)
						i.Value = cmd.Parameters[i.Name].Value;
				}
			}
			finally
			{
				if (!externalConnection)
					connection.Close();
			}
		}

		private void SetupParameters(IDataCommandDescriptor command, SqlCommand cmd)
		{
			if (cmd.Parameters.Count > 0)
			{
				foreach (SqlParameter i in cmd.Parameters)
					i.Value = DBNull.Value;
			}
			else
			{
				SqlParameter rv = null;

				foreach (var i in command.Parameters)
				{
					var p = new SqlParameter
					{
						ParameterName = i.Name,
					};

					if (i.Direction == ParameterDirection.ReturnValue)
						p.Direction = ParameterDirection.ReturnValue;

					cmd.Parameters.Add(p);

					if (i.Direction == ParameterDirection.ReturnValue && rv == null)
						rv = p;
				}
			}
		}

		public IDataConnection OpenConnection(string connectionString)
		{
			return new DataConnection(connectionString);
		}

		public JObject Query(IDataCommandDescriptor command, DataTable schema)
		{
			var con = new SqlConnection(command.ConnectionString);
			var com = new SqlCommand(command.CommandText, con)
			{
				CommandType = command.CommandType,
				CommandTimeout = command.CommandTimeout
			};

			con.Open();
			SqlDataReader rdr = null;

			try
			{
				foreach (var i in command.Parameters)
				{
					var p = new SqlParameter
					{
						ParameterName = i.Name,
						Value = i.Value == null || i.Value == DBNull.Value ? DBNull.Value : i.Value
					};

					com.Parameters.Add(p);
				}

				rdr = com.ExecuteReader();
				var r = new JObject();
				var a = new JArray();

				r.Add("data", a);

				while (rdr.Read())
				{
					var row = new JObject();

					foreach (DataColumn i in schema.Columns)
					{
						if (i.ExtendedProperties.Contains("unbound"))
						{
							row.Add(i.ColumnName, string.Empty);

							continue;
						}

						var mapping = i.ColumnName;

						if (i.ExtendedProperties.Contains("mapping"))
							mapping = (string)i.ExtendedProperties["mapping"];

						int ord = rdr.GetOrdinal(mapping);

						var value = rdr.GetValue(ord);

						row.Add(i.ColumnName, new JValue(value == DBNull.Value ? null : value));
					}

					a.Add(row);
				}

				return r;
			}
			finally
			{
				if (rdr != null && !rdr.IsClosed)
					rdr.Close();

				if (con.State == ConnectionState.Open)
					con.Close();
			}
		}

		private ReliableSqlConnection ResolveConnection(IDataCommandDescriptor command, IDataConnection connection)
		{
			DataConnection c = null;

			if (connection != null)
			{
				c = connection as DataConnection;

				if (c == null)
					throw new RuntimeException(string.Format(SR.ErrInvalidConnectionType, typeof(DataConnection).ShortName()));
			}

			return c == null
				? new ReliableSqlConnection(command.ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed)
				: c.Connection;
		}

		private SqlCommand ResolveCommand(IDataCommandDescriptor command, ReliableSqlConnection connection, IDataConnection dataConnection)
		{
			var commandKey = string.Format("{0}/{1}", command.CommandText, command.CommandType).ToLowerInvariant();
			DataConnection dc = null;

			if (dataConnection != null)
			{
				if (!(dataConnection is DataConnection))
					throw new RuntimeException(string.Format(SR.ErrInvalidConnectionType, typeof(DataConnection).ShortName()));

				dc = dataConnection as DataConnection;

				if (dc.Commands.ContainsKey(commandKey))
					return dc.Commands[commandKey];
			}

			var r = connection.CreateCommand();

			r.CommandText = command.CommandText;
			r.CommandType = command.CommandType;
			r.CommandTimeout = command.CommandTimeout;

			if (dc != null)
			{
				if (dc.Transaction != null)
					r.Transaction = dc.Transaction;

				dc.Commands.Add(commandKey, r);
			}

			return r;
		}

		public IDatabase CreateSchema(string connectionString)
		{
			return Package.Create(connectionString);
		}

		public void TestConnection(string connectionString)
		{
			using (var c = new SqlConnection(connectionString))
			{
				c.Open();
				c.Close();
			}
		}

		public void CreateDatabase(string connectionString)
		{
			var builder = new SqlConnectionStringBuilder(connectionString);

			var ic = builder.InitialCatalog;

			builder.InitialCatalog = string.Empty;

			using (var c = new SqlConnection(builder.ConnectionString))
			{
				c.Open();

				var com = new SqlCommand(string.Format("CREATE DATABASE {0}", ic), c);

				com.ExecuteNonQuery();

				c.Close();
			}
		}

		public void Deploy(IDatabaseDeploymentContext context)
		{
			var existing = CreateSchema(context.ConnectionString);

			new SqlDeploy(context, existing).Deploy();
		}
	}
}
