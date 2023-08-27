using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Data
{
    public abstract class DataConnectionBase : IDataConnection, IDisposable
    {
        private ICommandTextParser _parser = null;
        private IDbConnection _connection;
        private Lazy<SingletonProcessor<int>> _connectionProcessor = new Lazy<SingletonProcessor<int>>();

        protected DataConnectionBase(IMiddlewareContext context, IDataProvider provider, string connectionString, ConnectionBehavior behavior)
        {
            Context = context;
            Provider = provider;
            ConnectionString = connectionString;
            Behavior = behavior;
        }

        public IMiddlewareContext Context { get; }
        private IDataProvider Provider { get; }
        protected string ConnectionString { get; }
        private bool OwnsTransaction { get; set; }
        public IDbTransaction Transaction { get; set; }

        public ConnectionBehavior Behavior { get; private set; }

        private SingletonProcessor<int> ConnectionProcessor => _connectionProcessor.Value;
        public ICommandTextParser Parser
        {
            get
            {
                if (_parser is null)
                    _parser = OnCreateTextParser();

                return _parser;
            }
        }

        private bool Disposed { get; set; }

        public ConnectionState State => _connection is null || IsOpening ? ConnectionState.Closed : _connection.State;
        private bool IsOpening { get; set; }
        protected IDbConnection Connection => _connection;

        protected abstract Task<IDbConnection> OnCreateConnection();

        public async Task Commit()
        {
            if (Connection is null)
                return;

            if (Transaction is null || Transaction.Connection is null)
                return;

            await ConnectionProcessor.Start(1,
                async () =>
                {
                    if (Transaction is null || Transaction.Connection is null)
                        return;

                    await OnCommit();
                });
        }

        protected virtual async Task OnCommit()
        {
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;

            await Task.CompletedTask;
        }

        public async Task Rollback()
        {
            if (!OwnsTransaction)
                return;

            if (Transaction is null || Transaction.Connection is null)
                return;

            await ConnectionProcessor.Start(3,
                async () =>
                {
                    if (Transaction is null || Transaction.Connection is null)
                        return;

                    try
                    {
                        await OnRollback();
                    }
                    catch { }
                });

            await Task.CompletedTask;
        }

        protected virtual async Task OnRollback()
        {
            Transaction.Rollback();

            await Task.CompletedTask;
        }

        public async Task Open()
        {
            await EnsureConnection();

            if (Connection.State == ConnectionState.Open && Transaction is not null)
                return;

            await ConnectionProcessor.Start(0,
                async () =>
                {
                    if (Connection.State != ConnectionState.Closed)
                        return;

                    IsOpening = true;
                    await OnOpen();

                    if (Transaction?.Connection is not null)
                        return;

                    OwnsTransaction = true;
                    Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    IsOpening = false;
                });

            await Task.CompletedTask;
        }

        protected virtual async Task OnOpen()
        {
            Connection.Open();

            await Task.CompletedTask;
        }

        public async Task Close()
        {
            if (Connection is null)
                return;

            if (_connection is not null && _connection.State == ConnectionState.Open)
            {
                await ConnectionProcessor.Start(4,
                    async () =>
                    {
                        if (_connection is not null && _connection.State == ConnectionState.Open)
                        {
                            if (Transaction is not null && Transaction.Connection is not null)
                            {
                                try
                                {
                                    await OnRollback();
                                }
                                catch { }

                            }

                            await OnClose();
                        }
                    });
            }

            await Task.CompletedTask;
        }

        protected async Task OnClose()
        {
            Connection.Close();

            await Task.CompletedTask;
        }

        public async Task<int> Execute(IDataCommandDescriptor command)
        {
            return await Provider.Execute(Context, command, this);
        }

        public async Task<List<T>> Query<T>(IDataCommandDescriptor command)
        {
            return await Provider.Query<T>(Context, command, this);
        }

        public async Task<T> Select<T>(IDataCommandDescriptor command)
        {
            return await Provider.Select<T>(Context, command, this);
        }

        public IDbCommand? CreateCommand()
        {
            return Connection?.CreateCommand();
        }

        protected virtual ICommandTextParser? OnCreateTextParser() => null;

        private async Task EnsureConnection()
        {
            if (Connection is not null)
                return;

            if (Disposed)
                return;

            await ConnectionProcessor.Start(2,
                async () =>
                {
                    if (_connection is null && !Disposed)
                        _connection = await OnCreateConnection();
                });
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            AsyncUtils.RunSync(Close);

            if (Transaction is not null)
            {
                try
                {
                    //No way to check if possible
                    AsyncUtils.RunSync(Rollback);
                    Transaction.Dispose();
                }
                catch { }

                Transaction = null;
            }

            if (_connection is not null)
            {
                _connection.Dispose();
                _connection = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}