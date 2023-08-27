using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data
{
    internal class DataReader<T> : DataCommand, IDataReader<T>
    {
        public DataReader(IMiddlewareContext context) : base(context)
        {
        }

        public async Task<List<T>> Query()
        {
            try
            {
                EnsureCommand();

                var result = await Connection.Query<T>(Command);

                if (Connection.Behavior == ConnectionBehavior.Isolated)
                    await Connection.Commit();

                return result;
            }
            finally
            {
                if (Connection.Behavior == ConnectionBehavior.Isolated)
                {
                    await Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        public async Task<T> Select()
        {
            try
            {
                EnsureCommand();

                var result = await Connection.Select<T>(Command);

                if (Connection.Behavior == ConnectionBehavior.Isolated)
                    await Connection.Commit();

                return result;
            }
            finally
            {
                if (Connection.Behavior == ConnectionBehavior.Isolated)
                {
                    await Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }
    }
}
