using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.Sql;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.Sql
{
	internal class DataConnection : IDataConnection, IDisposable
	{
		private ReliableSqlConnection _connection = null;
		private Dictionary<string, SqlCommand> _commands = null;
		private List<DataConnection> _connections = null;

		public DataConnection(IDataProvider provider, string connectionString, IDataConnection existingConnection, IMiddlewareTransaction transaction)
		{
			ExistingConnection = existingConnection;
			Provider = provider;
			ConnectionString = connectionString;

			if (ExistingConnection != null && ExistingConnection is DataConnection dc && string.Compare(dc.ConnectionString, ConnectionString, true) == 0)
			{
				Attached = true;

				_connection = dc.Connection;
				Transaction = dc.Transaction;
			}

			MiddlewareTransaction = transaction;

			if (MiddlewareTransaction is IMiddlewareConnectionBag bag)
				bag.Push(this);

			Connections.Add(this);
		}

		private IMiddlewareTransaction MiddlewareTransaction { get; set; }
		private bool Completed { get; set; }
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
					_connection = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

					Open();
				}

				return _connection;
			}
		}

		public void Begin(IsolationLevel isolationLevel)
		{
			if (Connection.State == ConnectionState.Closed)
				Connection.Open();

			if (Transaction?.Connection != null)
			{
				return;
			}

			Transaction = Connection.BeginTransaction(isolationLevel) as SqlTransaction;

			foreach(var connection in Connections)
			{
				if (connection.Connection != Connection)
					continue;

				if (connection.Transaction == null)
					connection.Transaction = Transaction;
			}
		}

		public void Begin()
		{
			Begin(IsolationLevel.Unspecified);
		}

		public void Commit()
		{
			if (Completed)
				return;

			if (MiddlewareTransaction != null && MiddlewareTransaction.State == MiddlewareTransactionState.Active)
				return;

			if (!Attached && Transaction != null)
				CommitTransaction();
			else
			{
				foreach (var connection in Connections)
				{
					if (connection.Connection == Connection && !connection.Attached)
					{
						connection.IsCommittable = true;
						break;
					}
				}
			}


			Completed = true;
		}

		private bool IsCommittable { get; set; }

		public void Dispose()
		{
			Close();
		}

		public void Rollback()
		{
			if (Completed)
				return;

			if (MiddlewareTransaction != null && MiddlewareTransaction.State == MiddlewareTransactionState.Active)
				return;

			if (!Attached && Transaction != null)
				RollbackTransaction();

			Completed = true;
		}

		public void Open()
		{
			if (Connection.State == ConnectionState.Closed)
				Connection.Open();

			if (MiddlewareTransaction != null)
				Begin();
		}

		public void Close()
		{
			if (Attached)
				return;

			if (MiddlewareTransaction != null && MiddlewareTransaction.State == MiddlewareTransactionState.Active)
				return;

			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				if (Transaction != null)
				{
					if (IsCommittable)
						CommitTransaction();
					else
						RollbackTransaction();

					IsCommittable = false;
				}

				Connection.Close();
			}
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

		private List<DataConnection> Connections
		{
			get
			{
				if (_connections == null)
				{
					if (ExistingConnection != null && ExistingConnection is DataConnection dc)
						return dc.Connections;

					_connections = new List<DataConnection>();
				}

				return _connections;
			}
		}

		private void CommitTransaction()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			Transaction.Commit();
			Transaction = null;
		}

		private void RollbackTransaction()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			Transaction.Rollback();
			Transaction = null;
		}
	}
}
