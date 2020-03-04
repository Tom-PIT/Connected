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
		}

		private IMiddlewareTransaction MiddlewareTransaction { get; set; }
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

			if (Connection.State == ConnectionState.Closed)
				Connection.Open();

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

			if (MiddlewareTransaction != null && MiddlewareTransaction.State == MiddlewareTransactionState.Active)
				return;

			if (!Attached && Transaction != null)
				Transaction.Commit();

			Commited = true;
		}

		public void Dispose()
		{
			if (Connection != null)
			{
				if (!Attached)
				{
					if (Transaction != null)
						Transaction.Dispose();

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

			if (MiddlewareTransaction != null && MiddlewareTransaction.State == MiddlewareTransactionState.Active)
				return;

			if (!Attached && Transaction != null)
				Transaction.Rollback();

			Commited = true;
		}

		public void Open()
		{
			if (Attached)
				return;

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
