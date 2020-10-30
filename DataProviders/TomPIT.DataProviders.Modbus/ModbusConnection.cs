using System.Data;
using System.Data.Common;
using TomPIT.Serialization;

namespace TomPIT.DataProviders.Modbus
{
	internal class ModbusConnection : DbConnection
	{
		private ModbusClient _client = null;
		private ModbusConnectionString _cs = null;
		public override string ConnectionString { get; set; }

		public override string Database => ConnectionStringDescriptor.Device.ToString();

		public override string DataSource => ConnectionStringDescriptor.IpAddress;

		public override string ServerVersion => 0.ToString();

		public override ConnectionState State => Client.Connected ? ConnectionState.Open : ConnectionState.Closed;

		public override void ChangeDatabase(string databaseName)
		{

		}

		public override void Close()
		{
			if (State == ConnectionState.Open)
				Client.Disconnect();
		}

		public override void Open()
		{
			if (State == ConnectionState.Closed)
				Client.Connect();
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return null;
		}

		protected override DbCommand CreateDbCommand()
		{
			return new ModbusCommand
			{
				Connection = this
			};
		}

		internal ModbusClient Client
		{
			get
			{
				if (_client == null)
					_client = ModbusClient.Create(ConnectionStringDescriptor);

				return _client;
			}
		}

		private ModbusConnectionString ConnectionStringDescriptor
		{
			get
			{
				if (_cs == null)
					_cs = Serializer.Deserialize<ModbusConnectionString>(ConnectionString);

				return _cs;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_client != null)
			{
				_client.Disconnect();
				_client = null;
			}

			base.Dispose(disposing);
		}
	}
}
