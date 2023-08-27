using System;
using System.Collections.Immutable;
using System.Data;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data.Storage;

public enum StorageConnectionMode
{
    Shared = 1,
    Isolated
}

public interface IStorageConnection : Middleware.IMiddleware, IAsyncDisposable, IDisposable
{
    StorageConnectionMode Behavior { get; }
    string ConnectionString { get; }

    Task Initialize(IMiddlewareContext context, IConnectionProvider connections, StorageConnectionArgs args);
    Task Commit();
    Task Rollback();
    Task Close();

    Task<int> Execute(IStorageCommand command);

    Task<ImmutableList<T>> Query<T>(IStorageCommand command);

    Task<T?> Select<T>(IStorageCommand command);

    Task<IDataReader?> OpenReader(IStorageCommand command);
}
