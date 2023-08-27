using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.Data
{
    public interface IDataConnection : IDisposable
    {
        Task Commit();
        Task Rollback();

        Task Open();
        Task Close();

        Task<int> Execute(IDataCommandDescriptor command);

        Task<List<T>> Query<T>(IDataCommandDescriptor command);

        Task<T> Select<T>(IDataCommandDescriptor command);

        IDbCommand CreateCommand();
        ConnectionBehavior Behavior { get; }

        //IDbConnection Connection { get; }
        IDbTransaction Transaction { get; set; }
        ICommandTextParser Parser { get; }

        ConnectionState State { get; }
        IMiddlewareContext Context { get; }
    }
}
