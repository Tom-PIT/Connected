using Microsoft.Data.SqlClient;
using System.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.Sql
{
	public sealed class DataConnection : DataConnectionBase
	{
		public DataConnection(IMiddlewareContext context, IDataProvider provider, string connectionString, ConnectionBehavior behavior) : base(context, provider, connectionString, behavior)
		{
		}

		protected override IDbConnection OnCreateConnection()
		{
			return new SqlConnection(ConnectionString);
		}

		protected override ICommandTextParser OnCreateTextParser()
		{
			return new ProcedureTextParser();
		}

		protected override void OnRollback()
		{
			((SqlTransaction)Transaction).Rollback();
		}

		protected override void OnCommit()
		{
			((SqlTransaction)Transaction).Commit();
		}

		protected override void OnOpen()
		{
			((SqlConnection)Connection).Open();
		}
	}
}
