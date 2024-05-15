using System;
using System.Collections.Generic;
using System.Data;
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

		protected abstract IDbConnection OnCreateConnection();

		public void Commit()
		{
			if (Connection is null)
				return;

			if (Transaction is null || Transaction.Connection is null)
				return;

			ConnectionProcessor.Start(1,
				 () =>
				 {
					 if (Transaction is null || Transaction.Connection is null)
						 return;

					 OnCommit();
				 });
		}

		protected virtual void OnCommit()
		{
			Transaction.Commit();
			Transaction.Dispose();
			Transaction = null;
		}

		public void Rollback()
		{
			if (!OwnsTransaction)
				return;

			if (Transaction is null || Transaction.Connection is null)
				return;

			ConnectionProcessor.Start(3,
				 () =>
				 {
					 if (Transaction is null || Transaction.Connection is null)
						 return;

					 try
					 {
						 OnRollback();
					 }
					 catch { }
				 });
		}

		protected virtual void OnRollback()
		{
			Transaction.Rollback();
			Transaction.Dispose();
			Transaction = null;
		}

		public void Open()
		{
			EnsureConnection();

			if (Connection.State == ConnectionState.Open && Transaction is not null)
				return;

			ConnectionProcessor.Start(0,
				 () =>
				 {
					 if (Connection.State != ConnectionState.Closed)
						 return;

					 IsOpening = true;
					 OnOpen();

					 if (Transaction?.Connection is not null)
						 return;

					 OwnsTransaction = true;
					 Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
					 IsOpening = false;
				 });
		}

		protected virtual void OnOpen()
		{
			Connection.Open();
		}

		public void Close()
		{
			if (Connection is null)
				return;

			if (_connection is not null && _connection.State == ConnectionState.Open)
			{
				ConnectionProcessor.Start(4,
					 () =>
					 {
						 if (_connection is not null && _connection.State == ConnectionState.Open)
						 {
							 if (Transaction is not null && Transaction.Connection is not null)
							 {
								 try
								 {
									 OnRollback();
								 }
								 catch { }

							 }

							 OnClose();
						 }
					 });
			}
		}

		protected void OnClose()
		{
			Connection.Close();
		}

		public int Execute(IDataCommandDescriptor command)
		{
			return Provider.Execute(Context, command, this);
		}

		public List<T> Query<T>(IDataCommandDescriptor command)
		{
			return Provider.Query<T>(Context, command, this);
		}

		public T Select<T>(IDataCommandDescriptor command)
		{
			return Provider.Select<T>(Context, command, this);
		}

		public IDbCommand? CreateCommand()
		{
			return Connection?.CreateCommand();
		}

		protected virtual ICommandTextParser? OnCreateTextParser() => null;

		private void EnsureConnection()
		{
			if (Connection is not null)
				return;

			if (Disposed)
				return;

			ConnectionProcessor.Start(2,
				 () =>
				 {
					 if (_connection is null && !Disposed)
						 _connection = OnCreateConnection();
				 });
		}

		public void Dispose()
		{
			if (Disposed)
				return;

			Disposed = true;
			Close();

			if (Transaction is not null)
			{
				try
				{
					//No way to check if possible
					Rollback();
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