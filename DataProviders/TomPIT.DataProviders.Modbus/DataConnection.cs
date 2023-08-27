using System.Data;
using System.Threading.Tasks;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.Modbus
{
    public sealed class DataConnection : DataConnectionBase
    {
        public DataConnection(IMiddlewareContext context, IDataProvider provider, string connectionString, ConnectionBehavior behavior) : base(context, provider, connectionString, behavior)
        {
        }

        protected override async Task<IDbConnection> OnCreateConnection()
        {
            await Task.CompletedTask;

            return new ModbusConnection
            {
                ConnectionString = ConnectionString
            };
        }
    }
}
