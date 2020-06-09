namespace TomPIT.DataProviders.Modbus
{
	public enum ModbusFieldType
	{
		Bool = 1,
		Byte = 2,
		Short = 3,
		Int = 4,
		Long = 5,
		Float = 6,
		String = 7
	}
	internal class ModbusCommandField
	{
		public string Name { get; set; }
		public int Offset { get; set; }
		public int Length { get; set; }
		public ModbusFieldType DataType { get; set; } = ModbusFieldType.Short;
	}
}
