using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.Sql;

namespace TomPIT.DataProviders.Sql
{
	internal class DataConnection : IDataConnection, IDisposable
	{
		private ReliableSqlConnection _connection = null;
		private SqlTransaction _transaction = null;
		private Dictionary<string, SqlCommand> _commands = null;
		private bool _commited = false;

		public DataConnection(IDataProvider provider, string connectionString)
		{
			Provider = provider;
			ConnectionString = connectionString;
		}

		private IDataProvider Provider { get; }
		private string ConnectionString { get; }

		public ReliableSqlConnection Connection
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

		public void Begin(IsolationLevel isolationLevel)
		{
			if (_transaction != null)
				return;

			_transaction = Connection.BeginTransaction(isolationLevel) as SqlTransaction;
		}

		public void Begin()
		{
			Begin(IsolationLevel.Unspecified);
		}

		public void Commit()
		{
			if (_commited)
				return;

			if (_transaction != null)
				_transaction.Commit();

			_commited = true;
		}

		public void Dispose()
		{
			if (Connection != null)
			{
				if (_transaction != null)
					_transaction.Dispose();

				if (Connection.State == ConnectionState.Open)
					Connection.Close();

				Connection.Dispose();
			}
		}

		public void Rollback()
		{
			if (_commited)
				return;

			if (_transaction != null)
				_transaction.Rollback();

			_commited = true;
		}

		public void Open()
		{
			if (Connection.State == ConnectionState.Closed)
				Connection.Open();
		}

		public void Close()
		{
			if (Connection.State == ConnectionState.Open)
				Connection.Close();
		}

		public void Execute(IDataCommandDescriptor command)
		{
			Provider.Execute(command, this);
		}

		public JObject Query(IDataCommandDescriptor command)
		{
			return Provider.Query(command, null, this);
		}

		public Dictionary<string, SqlCommand> Commands
		{
			get
			{
				if (_commands == null)
					_commands = new Dictionary<string, SqlCommand>();

				return _commands;
			}
		}

		public SqlTransaction Transaction { get { return _transaction; } }
	}
}
