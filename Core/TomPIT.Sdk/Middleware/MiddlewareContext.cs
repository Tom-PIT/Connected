using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.Storage;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using TomPIT.Middleware.Storage;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Security;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
    public class MiddlewareContext : IMiddlewareContext, IElevationContext, IIOBehaviorContext, IDisposable
    {
        #region Members
        private ElevationContextState _elevationState = ElevationContextState.Revoked;
        private object _authorizationOwner;
        private CancellationTokenSource _cancellationTokenSource = new();
        #endregion

        #region Constructors
        public MiddlewareContext()
        {
            Transactions = new TransactionContext();
            Interop = new MiddlewareInterop(this);
            Services = new MiddlewareServices(this, Transactions);
            Environment = new MiddlewareEnvironment();
            Connections = new ConnectionProvider(this, Transactions);
            ModelConnections = new(this, Transactions);
            Tenant = MiddlewareDescriptor.Current.Tenant;

            _authorizationOwner = this;

            Initialize();
        }

        public MiddlewareContext(bool interactive) : this()
        {
            ((MiddlewareEnvironment)Environment).IsInteractive = interactive;
        }
        #endregion

        #region Properties
        private bool Disposed { get; set; }

        private bool IsReadOnly => ((IIOBehaviorContext)this).Behavior == EnvironmentIOBehavior.ReadOnly;
        internal ITransactionContext Transactions { get; }
        private IConnectionProvider Connections { get; }
        private MiddlewareConnectionPool ModelConnections { get; }
        [JsonIgnore]
        ElevationContextState IElevationContext.State { get; set; } = ElevationContextState.Revoked;

        [JsonIgnore]
        object IElevationContext.AuthorizationOwner { get { return _authorizationOwner; } set { _authorizationOwner = value; } }

        [JsonIgnore]
        public virtual IMiddlewareServices Services { get; }

        [JsonIgnore]
        public IMiddlewareEnvironment Environment { get; }

        [JsonIgnore]
        public ITenant Tenant { get; }

        [JsonIgnore]
        public IMiddlewareInterop Interop { get; }

        [JsonIgnore]
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        EnvironmentIOBehavior IIOBehaviorContext.Behavior { get; set; }

        #endregion

        #region Methods

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
        public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            return AsyncUtils.RunSync(() => OpenReaderAsync<T>(connection, commandText));
        }

        public async Task<IDataReader<T>> OpenReaderAsync<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            if (Transactions.State == MiddlewareTransactionState.Active)
                return await OpenReaderAsync<T>(connection, commandText, ConnectionBehavior.Shared);
            else
                return await OpenReaderAsync<T>(connection, commandText, ConnectionBehavior.Isolated);
        }

        public IDataReader<T> OpenReader<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
        {
            return AsyncUtils.RunSync(() => OpenReaderAsync<T>(connection, commandText, behavior));
        }

        public async Task<IDataReader<T>> OpenReaderAsync<T>([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
        {
            return new DataReader<T>(this)
            {
                Connection = await OpenConnection(connection, behavior),
                CommandText = commandText
            };
        }

        public IDataReader<T> OpenReader<T>(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            return new DataReader<T>(this)
            {
                Connection = connection,
                CommandText = commandText
            };
        }

        public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            return AsyncUtils.RunSync(() => OpenWriterAsync(connection, commandText));
        }

        public async Task<IDataWriter> OpenWriterAsync([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            if (IsReadOnly)
                throw new TomPITException(SR.ErrContextInReadonlyMode);

            if (Transactions.State == MiddlewareTransactionState.Active)
                return await OpenWriterAsync(connection, commandText, ConnectionBehavior.Shared);
            else
                return await OpenWriterAsync(connection, commandText, ConnectionBehavior.Isolated);
        }

        public IDataWriter OpenWriter(IDataConnection connection, [CIP(CIP.CommandTextProvider)] string commandText)
        {
            if (IsReadOnly)
                throw new TomPITException(SR.ErrContextInReadonlyMode);

            return new DataWriter(this, Transactions)
            {
                Connection = connection,
                CommandText = commandText
            };
        }

        public IDataWriter OpenWriter([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
        {
            return AsyncUtils.RunSync(() => OpenWriterAsync(connection, commandText, behavior));
        }

        public async Task<IDataWriter> OpenWriterAsync([CIP(CIP.ConnectionProvider)] string connection, [CIP(CIP.CommandTextProvider)] string commandText, ConnectionBehavior behavior)
        {
            if (IsReadOnly)
                throw new TomPITException(SR.ErrContextInReadonlyMode);

            return new DataWriter(this, Transactions)
            {
                Connection = await OpenConnection(connection, behavior),
                CommandText = commandText
            };
        }

        public void MarkUnstable()
        {
            _cancellationTokenSource.Cancel(true);
        }
        #endregion

        protected void Initialize()
        {
            ((IIOBehaviorContext)this).Behavior = Tenant.GetService<IRuntimeService>().IOBehavior;
        }

        internal async Task<IDataConnection> OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior)
        {
            return await OpenConnectionAsync(connection, behavior, null);
        }

        public async Task<IDataConnection> OpenConnectionAsync([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior, object arguments)
        {
            try
            {
                CancellationToken.ThrowIfCancellationRequested();

                return await ModelConnections.OpenConnection(connection, behavior, arguments);
            }
            catch (OperationCanceledException ex)
            {
                //TODO localize
                throw new Exception("Cannot open new connection on unstable context", ex);
            }
            catch
            {
                throw;
            }
        }

        public IDataConnection OpenConnection([CIP(CIP.ConnectionProvider)] string connection, ConnectionBehavior behavior, object arguments)
        {
            return AsyncUtils.RunSync(() => OpenConnectionAsync(connection, behavior, arguments));
        }

        public T OpenModel<T>() where T : IModelComponent
        {
            var result = TypeExtensions.CreateInstance<IModelComponent>(typeof(T));

            ReflectionExtensions.SetPropertyValue(result, nameof(result.Context), this);

            return (T)result;
        }

        public async Task<IStorage<TEntity>> OpenStorage<TEntity>() where TEntity : IEntity
        {
            var result = new EntityStorage<TEntity>(this, Connections, Transactions);

            await result.Initialize();

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Connections.Dispose();
                    ModelConnections.Dispose();
                    Interop.Dispose();
                    Services.Dispose();
                }

                Disposed = true;
            }
        }

        ~MiddlewareContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
