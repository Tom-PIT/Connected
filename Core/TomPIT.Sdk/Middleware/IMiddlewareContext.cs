using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.Storage;
using TomPIT.Middleware.Services;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
    public interface IMiddlewareContext : IDisposable
    {
        IMiddlewareServices Services { get; }
        IMiddlewareEnvironment Environment { get; }
        ITenant Tenant { get; }
        IMiddlewareInterop Interop { get; }

        CancellationToken CancellationToken { get; }

        void Cancel();
        IDataReader<T> OpenReader<T>(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText);
        [Obsolete("Please use async method.")]
        IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText);
        [Obsolete("Please use async method.")]
        IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText);
        IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText);
        [Obsolete("Please use async method.")]
        IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior);
        [Obsolete("Please use async method.")]
        IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior);
        [Obsolete("Please use async method.")]
        IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior, object arguments);

        Task<IDataReader<T>> OpenReaderAsync<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText);
        Task<IDataWriter> OpenWriterAsync([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText);

        Task<IDataReader<T>> OpenReaderAsync<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior);
        Task<IDataWriter> OpenWriterAsync([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior);
        Task<IDataConnection> OpenConnectionAsync([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior, object arguments);

        T OpenModel<T>() where T : IModelComponent;
        Task<IStorage<T>> OpenStorage<T>() where T : IEntity;
    }
}