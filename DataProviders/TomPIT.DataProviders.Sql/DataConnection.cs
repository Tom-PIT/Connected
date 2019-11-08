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
		private Dictionary<string, SqlCommand> _commands = null;

		public DataConnection(IDataProvider provider, string connectionString, IDataConnection existingConnection)
		{
			ExistingConnection = existingConnection;
			Provider = provider;
			ConnectionString = connectionString;
		}

		private bool Commited { get; set; }
		private bool Attached { get; set; }
		private IDataConnection ExistingConnection { get; }
		private IDataProvider Provider { get; }
		private string ConnectionString { get; }

		public ReliableSqlConnection Connection
		{
			get
			{
				if (_connection == null)
				{
					if (ExistingConnection != null)
					{
						if (ExistingConnection is DataConnection dc && string.Compare(dc.ConnectionString, ConnectionString, true) == 0)
						{
							Attached = true;
							_connection = dc.Connection;
							Transaction = dc.Transaction;
						}
					}
					else
						_connection = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

					Open();
				}

				return _connection;
			}
		}

		public void Begin(IsolationLevel isolationLevel)
		{
			if (Transaction != null)
				return;

			Transaction = Connection.BeginTransaction(isolationLevel) as SqlTransaction;
		}

		public void Begin()
		{
			Begin(IsolationLevel.Unspecified);
		}

		public void Commit()
		{
			if (Commited)
				return;

			if (Transaction != null)
				Transaction.Commit();

			Commited = true;
		}

		public void Dispose()
		{
			if (Connection != null)
			{
				if (Transaction != null)
					Transaction.Dispose();

				if (!Attached)
				{
					if (Connection.State == ConnectionState.Open)
						Connection.Close();

					Connection.Dispose();
				}
			}
		}

		public void Rollback()
		{
			if (Commited)
				return;

			if (Transaction != null)
				Transaction.Rollback();

			Commited = true;
		}

		public void Open()
		{
			if (Attached)
				return;

			if (Connection.State == ConnectionState.Closed)
				Connection.Open();
		}

		public void Close()
		{
			if (Attached)
				return;

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

		public SqlTransaction Transaction { get; private set; }
	}
}
