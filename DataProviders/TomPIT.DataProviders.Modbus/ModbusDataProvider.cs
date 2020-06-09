using System;
using System.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Modbus
{
	public class ModbusDataProvider : DataProviderBase<DataConnection>
	{
		public ModbusDataProvider() : base("Modbus", new Guid("06A3E378-8602-4B9C-B18A-A4D0CBC32038"))
		{

		}
		public override IDataConnection OpenConnection(string connectionString, ConnectionBehavior behavior)
		{
			return new DataConnection(this, connectionString, behavior);
		}

		protected override IDbConnection CreateConnection(string connectionString)
		{
			return new ModbusConnection
			{
				ConnectionString = connectionString
			};
		}
	}
}
