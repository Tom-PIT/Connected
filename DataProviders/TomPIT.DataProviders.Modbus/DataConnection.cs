using System;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.Modbus
{
	public sealed class DataConnection : IDataConnection, IDisposable
	{
		private ModbusConnection _connection = null;

		public DataConnection(IMiddlewareContext context, IDataProvider provider, string connectionString, ConnectionBehavior behavior)
		{
			Context = context;
			Provider = provider;
			ConnectionString = connectionString;
			Behavior = behavior;
		}

		public IMiddlewareContext Context { get; }
		private IDataProvider Provider { get; }
		private string ConnectionString { get; }

		public IDbTransaction Transaction { get; set; }
		public IDbConnection Connection
		{
			get
			{
				if (_connection == null)
				{
					_connection = new ModbusConnection
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
		}

		public void Dispose()
		{
			Close();
		}

		public void Rollback()
		{
		}

		public void Open()
		{
			if (Connection.State == ConnectionState.Closed)
				Connection.Open();
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

		public IDbCommand CreateCommand()
		{
			return Connection.CreateCommand();
		}

		public ConnectionBehavior Behavior { get; private set; }

		public ICommandTextParser Parser => null;

		public ConnectionState State => Connection.State;
	}
}
