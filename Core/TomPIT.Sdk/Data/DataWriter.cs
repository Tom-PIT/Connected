using System;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Data.Storage;
using TomPIT.Middleware;

namespace TomPIT.Data
{
    internal class DataWriter : DataCommand, IDataWriter
    {
        public DataWriter(IMiddlewareContext context, ITransactionContext transactions) : base(context)
        {
            Transactions = transactions;
        }
        private ITransactionContext Transactions { get; }
        public async Task<int> Execute()
        {
            try
            {
                EnsureCommand();
                var recordsAffected = await Connection.Execute(Command);

                if (Connection.Behavior == ConnectionBehavior.Isolated)
                    await Connection.Commit();

                BindReturnValues();

                SignalDirty();

                return recordsAffected;
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

        public async Task<T> Execute<T>()
        {
            try
            {
                EnsureCommand();
                await Connection.Execute(Command);

                if (Connection.Behavior == ConnectionBehavior.Isolated)
                    await Connection.Commit();

                BindReturnValues();
                SignalDirty();

                foreach (var parameter in Parameters)
                {
                    if (parameter.Direction == System.Data.ParameterDirection.ReturnValue
                        && Types.TryConvert(parameter.Value, out T r))
                        return r;
                }

                return default;
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

        private void BindReturnValues()
        {
            foreach (var parameter in Command.Parameters)
            {
                if (parameter.Direction == System.Data.ParameterDirection.ReturnValue)
                {
                    if (parameter.Value == DBNull.Value)
                        continue;

                    var par = Parameters.FirstOrDefault(f => string.Compare(parameter.Name, f.Name, true) == 0);

                    if (par == null)
                        continue;

                    if (Types.TryConvert(parameter.Value, out object r, Types.ToType(par.Type)))
                        par.Value = r;
                }
            }
        }

        public IDataParameter SetReturnValueParameter(string name)
        {
            var parameter = SetParameter(name, DBNull.Value);

            parameter.Direction = System.Data.ParameterDirection.ReturnValue;

            return parameter;
        }

        private void SignalDirty()
        {
            Transactions.IsDirty = true;
        }
    }
}
