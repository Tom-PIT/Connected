using System;
using System.Data;
using System.Data.SqlClient;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;
using TomPIT.Data.Sql;
using TomPIT.DataProviders.Sql.Deployment;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql
{
	[SchemaBrowser("TomPIT.DataProviders.Sql.Design.Browser, TomPIT.DataProviders.Sql")]
	public class SqlDataProvider : DataProviderBase<DataConnection>, IDeployDataProvider
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
						DbType = ResolveType(i)
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

		protected override void SetParameterValue(IDbCommand command, string parameterName, object value)
		{
			var cmd = command as SqlCommand;

			cmd.Parameters[parameterName].Value = value;
		}
		public override IDataConnection OpenConnection(string connectionString, ConnectionBehavior behavior)
		{
			return new DataConnection(this, connectionString, behavior);
		}

		protected override IDbConnection CreateConnection(string connectionString)
		{
			return new ReliableSqlConnection(connectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);
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
