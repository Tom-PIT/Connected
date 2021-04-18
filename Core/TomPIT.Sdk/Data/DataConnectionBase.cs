using System;
using System.Collections.Generic;
using System.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public abstract class DataConnectionBase: IDataConnection, IDisposable
	{
		private ICommandTextParser _parser = null;
		private IDbConnection _connection;
		private object _sync = new object();

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

		private bool Disposed { get; set; }

		public ConnectionState State => _connection == null ? ConnectionState.Closed : _connection.State;

		private IDbConnection Connection
		{
			get
			{
				if (_connection == null && !Disposed)
				{
					lock (_sync)
					{
						if (_connection == null && !Disposed)
							_connection = OnCreateConnection();
					}
				}

				return _connection;
			}
		}

		protected abstract IDbConnection OnCreateConnection();

		public void Commit()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			lock (_sync)
			{
				if (Transaction == null || Transaction.Connection == null)
					return;

				Transaction.Commit();
				Transaction.Dispose();
				Transaction = null;
			}
		}

		public void Dispose()
		{
			if (Disposed)
				return;

			Disposed = true;
			Close();

			if (Transaction != null)
			{
				try
				{
					Transaction.Dispose();
				}
				catch { }

				Transaction = null;
			}

			if (_connection != null)
			{
				_connection.Dispose();
				_connection = null;
			}

			Provider.Dispose();

			GC.SuppressFinalize(this);
		}

		public void Rollback()
		{
			if (!OwnsTransaction)
				return;

			if (Transaction == null || Transaction.Connection == null)
				return;

			lock (_sync)
			{
				if (Transaction == null || Transaction.Connection == null)
					return;

				try
				{
					Transaction.Rollback();
				}
				catch { }
			}
		}

		public void Open()
		{
			if (Connection.State == ConnectionState.Open)
				return;

			lock (_sync)
			{
				if (Connection.State != ConnectionState.Closed)
					return;

				Connection.Open();

				if (Transaction?.Connection != null)
					return;

				OwnsTransaction = true;
				Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		public void Close()
		{
			if (_connection == null)
				return;

			if (_connection != null && _connection.State == ConnectionState.Open)
			{
				lock (_sync)
				{
					if (_connection != null && _connection.State == ConnectionState.Open)
					{
						if (Transaction != null && Transaction.Connection != null)
						{
							try
							{
								Transaction.Rollback();
							}
							catch { }

						}
						_connection.Close();
					}
				}
			}
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

		public IDbCommand CreateCommand()
		{
			return Connection?.CreateCommand();
		}

		public IDbTransaction Transaction { get; set; }

		public ConnectionBehavior Behavior { get; private set; }

		public ICommandTextParser Parser
		{
			get
			{
				if (_parser == null)
					_parser = OnCreateTextParser();

				return _parser;
			}
		}

		protected virtual ICommandTextParser OnCreateTextParser() => null;
	}
}