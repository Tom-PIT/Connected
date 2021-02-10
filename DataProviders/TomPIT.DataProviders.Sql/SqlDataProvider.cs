using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;
using TomPIT.DataProviders.Sql.Deployment;
using TomPIT.DataProviders.Sql.Synchronization;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.Sql
{
	[SchemaBrowser("TomPIT.DataProviders.Sql.Design.Browser, TomPIT.DataProviders.Sql")]
	public class SqlDataProvider : DataProviderBase<DataConnection>, IDeployDataProvider, IOrmProvider
	{
		public SqlDataProvider() : base("Microsoft SQL Server", new Guid("{C5849300-11A4-4FAE-B433-3C89DD05DDF0}"))
		{

		}

		public bool SupportsDeploy => true;

		protected override void SetupParameters(IDataCommandDescriptor command, IDbCommand cmd)
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
						DbType = i.DataType
					};

					if (i.Direction == ParameterDirection.ReturnValue)
						p.Direction = ParameterDirection.ReturnValue;

					cmd.Parameters.Add(p);

					if (i.Direction == ParameterDirection.ReturnValue && rv == null)
						rv = p;
				}
			}
		}

		protected override object GetParameterValue(IDbCommand command, string parameterName)
		{
			var cmd = command as SqlCommand;

			return cmd.Parameters[parameterName].Value;
		}

		protected override void SetParameterValue(IDataConnection connection, IDbCommand command, string parameterName, object value)
		{
			var cmd = command as SqlCommand;

			if (value is DateTime date)
				value = connection.Context.Services.Globalization.ToUtc(date);

			cmd.Parameters[parameterName].Value = value;
		}
		public override IDataConnection OpenConnection(IMiddlewareContext context, string connectionString, ConnectionBehavior behavior)
		{
			return new DataConnection(context, this, connectionString, behavior);
		}

		public IDatabase CreateSchema(string connectionString)
		{
			return Package.Create(connectionString);
		}

		public void CreateDatabase(string connectionString)
		{
			var builder = new SqlConnectionStringBuilder(connectionString);

			var ic = builder.InitialCatalog;

			builder.InitialCatalog = string.Empty;

			using var c = new SqlConnection(builder.ConnectionString);

			c.Open();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
			var com = new SqlCommand(string.Format("CREATE DATABASE {0}", ic), c);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

			com.ExecuteNonQuery();

			c.Close();
		}

		public void Deploy(IDatabaseDeploymentContext context)
		{
			var existing = CreateSchema(context.ConnectionString);

			new SqlDeploy(context, existing).Deploy();
		}

		public void Synchronize(string connectionString, List<IModelSchema> models, List<IModelOperationSchema> views, List<IModelOperationSchema> procedures)
		{
			var sync = new Synchronizer(connectionString, models, views, procedures);

			sync.Execute();
		}

		public ICommandTextDescriptor Parse(string connectionString, IModelOperationSchema operation)
		{
			return new ProcedureTextParser().Parse(operation.Text);
		}
	}
}
