namespace TomPIT.DataProviders.Modbus
{
	internal class ModbusConnectionString
	{
		public string IpAddress { get; set; }
		public int Port { get; set; } = 502;
		public byte Device { get; set; }
		public string SerialPort { get; set; }
	}
}
