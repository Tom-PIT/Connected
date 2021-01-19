using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Sql
{
	public sealed class DataConnection : IDataConnection, IDisposable
	{
		private SqlConnection _connection = null;
		private ICommandTextParser _parser = null;
		private object _sync = new object();

		public DataConnection(IDataProvider provider, string connectionString, ConnectionBehavior behavior)
		{
			Provider = provider;
			ConnectionString = connectionString;
			Behavior = behavior;
		}

		private IDataProvider Provider { get; }
		private string ConnectionString { get; }
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
							_connection = new SqlConnection(ConnectionString);
					}
				}

				return _connection;
			}
		}

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
				Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted) as SqlTransaction;
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
			return Provider.Execute(command, this);
		}

		public JObject Query(IDataCommandDescriptor command)
		{
			return Provider.Query(command, null, this);
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
					_parser = new ProcedureTextParser();

				return _parser;
			}
		}
	}
}
