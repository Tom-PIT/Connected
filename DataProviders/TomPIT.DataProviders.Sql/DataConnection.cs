using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
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

        protected override async Task<IDbConnection> OnCreateConnection()
        {
            await Task.CompletedTask;

            return new SqlConnection(ConnectionString);
        }

        protected override ICommandTextParser OnCreateTextParser()
        {
            return new ProcedureTextParser();
        }

        protected override async Task OnRollback()
        {
            await ((SqlTransaction)Transaction).RollbackAsync();
        }

        protected override async Task OnCommit()
        {
            await ((SqlTransaction)Transaction).CommitAsync();
        }

        protected override async Task OnOpen()
        {
            await ((SqlConnection)Connection).OpenAsync();
        }
    }
}
