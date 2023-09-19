using System.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.BigData
{
	public sealed class DataConnection : DataConnectionBase
	{
		public DataConnection(IMiddlewareContext context, IDataProvider provider, string connectionString, ConnectionBehavior behavior) : base(context, provider, connectionString, behavior)
		{
		}

		protected override IDbConnection OnCreateConnection()
		{
			return new BigDataConnection
			{
				ConnectionString = ConnectionString
			};
		}
	}
}
