﻿using System;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.Modbus
{
	public class ModbusDataProvider : DataProviderBase<DataConnection>
	{
		public ModbusDataProvider() : base("Modbus", new Guid("06A3E378-8602-4B9C-B18A-A4D0CBC32038"))
		{

		}
		public override IDataConnection OpenConnection(IMiddlewareContext context, string connectionString, ConnectionBehavior behavior)
		{
			return new DataConnection(context, this, connectionString, behavior);
		}
	}
}
