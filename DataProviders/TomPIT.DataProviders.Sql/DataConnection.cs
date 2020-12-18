using System;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.Sql;

namespace TomPIT.DataProviders.Sql
{
	public sealed class DataConnection : IDataConnection, IDisposable
	{
		private ReliableSqlConnection _connection = null;
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

		private bool Disposed { get; set; }
		public IDbConnection Connection
		{
			get
			{
				if (_connection == null && !Disposed)
				{
					lock (_sync)
					{
						if (_connection == null && !Disposed)
						{
							_connection = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

							Open();
						}
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
				Transaction = null;
			}
		}

		public void Dispose()
		{
			Disposed = true;
			Close();
		}

		public void Rollback()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			lock (_sync)
			{
				if (Transaction == null || Transaction.Connection == null)
					return;

				Transaction.Rollback();
				Transaction = null;
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
				{
					return;
				}

				Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted) as SqlTransaction;
			}
		}

		public void Close()
		{
			if (_connection == null)
				return;

			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				lock (_sync)
				{
					if (Connection != null && Connection.State == ConnectionState.Open)
					{
						Rollback();
						Connection.Close();
					}
				}
			}

			if (_connection != null)
			{
				_connection.Dispose();
				_connection = null;
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
