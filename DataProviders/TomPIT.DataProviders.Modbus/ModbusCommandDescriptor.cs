using System.Collections.Generic;

namespace TomPIT.DataProviders.Modbus
{
	public enum ModbusCommandType
	{
		NotSet = 0,
		Coil = 1,
		Discrete = 2,
		Holding = 3,
		Input = 4
	}
	internal class ModbusCommandDescriptor
	{
		private List<ModbusCommandField> _fields = null;

		public ModbusCommandType Type { get; set; }
		public int Offset { get; set; }
		public int Length { get; set; }

		public List<ModbusCommandField> Fields
		{
			get
			{
				if (_fields == null)
					_fields = new List<ModbusCommandField>();

				return _fields;
			}
		}
	}
}
