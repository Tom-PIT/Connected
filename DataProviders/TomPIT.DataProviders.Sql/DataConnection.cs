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

		public DataConnection(IDataProvider provider, string connectionString, ConnectionBehavior behavior)
		{
			Provider = provider;
			ConnectionString = connectionString;
			Behavior = behavior;
		}

		private IDataProvider Provider { get; }
		private string ConnectionString { get; }

		public IDbConnection Connection
		{
			get
			{
				if (_connection == null)
				{
					_connection = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

					Open();
				}

				return _connection;
			}
		}

		public void Commit()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			lock (Transaction)
			{
				if (Transaction == null || Transaction.Connection == null)
					return;

				Transaction.Commit();
				Transaction = null;
			}
		}

		public void Dispose()
		{
			Close();
		}

		public void Rollback()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			lock (Transaction)
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

			lock (Connection)
			{
				if (Connection.State != ConnectionState.Closed)
					return;

				Connection.Open();

				if (Transaction?.Connection != null)
				{
					return;
				}

				Transaction = Connection.BeginTransaction(IsolationLevel.Unspecified) as SqlTransaction;
			}
		}

		public void Close()
		{
			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				lock (Connection)
				{
					if (Connection != null && Connection.State == ConnectionState.Open)
					{
						Rollback();
						Connection.Close();
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

		public IDbTransaction Transaction { get; set; }

		public ConnectionBehavior Behavior { get; private set; }

		public ICommandTextParser Parser
		{
			get
			{
				if (_parser == null)
					_parser = new CommandTextParser();

				return _parser;
			}
		}
	}
}
