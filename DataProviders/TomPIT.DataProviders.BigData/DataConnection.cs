using System;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.BigData
{
	public sealed class DataConnection : IDataConnection, IDisposable
	{
		private BigDataConnection _connection = null;

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
					_connection = new BigDataConnection
					{
						ConnectionString = ConnectionString
					};

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

			Transaction = Connection.BeginTransaction(IsolationLevel.Unspecified) as BigDataTransaction;
		}

		public void Close()
		{
			if (Connection != null && Connection.State == ConnectionState.Open)
				Connection.Close();
		}

		public int Execute(IDataCommandDescriptor command)
		{
			return Provider.Execute(command, this);
		}

		public JObject Query(IDataCommandDescriptor command)
		{
			return Provider.Query(command, null, this);
		}

		public ConnectionBehavior Behavior { get; private set; }
		public IDbTransaction Transaction { get; set; }

		public ICommandTextParser Parser => null;
	}
}
