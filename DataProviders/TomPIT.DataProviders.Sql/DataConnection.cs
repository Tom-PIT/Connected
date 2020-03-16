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

		public void Commit()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			Transaction.Commit();
			Transaction = null;
		}

		public void Dispose()
		{
			Close();
		}

		public void Rollback()
		{
			if (Transaction == null || Transaction.Connection == null)
				return;

			Transaction.Rollback();
			Transaction = null;
		}

		public void Open()
		{
			if (Connection.State == ConnectionState.Closed)
				Connection.Open();

			if (Transaction?.Connection != null)
			{
				return;
			}

			Transaction = Connection.BeginTransaction(IsolationLevel.Unspecified) as SqlTransaction;
		}

		public void Close()
		{
			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				Rollback();
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
	}
}
