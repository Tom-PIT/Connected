using System;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Modbus
{
	public sealed class DataConnection : IDataConnection, IDisposable
	{
		private ModbusConnection _connection = null;

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

		public void Execute(IDataCommandDescriptor command)
		{
			Provider.Execute(command, this);
		}

		public JObject Query(IDataCommandDescriptor command)
		{
			return Provider.Query(command, null, this);
		}

		public ConnectionBehavior Behavior { get; private set; }
	}
}
