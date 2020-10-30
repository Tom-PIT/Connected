using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq;
using TomPIT.Reflection;

namespace TomPIT.DataProviders.Modbus
{
	internal class ModbusReader : DbDataReader
	{
		private DataTable _schema = null;
		public ModbusReader(ModbusCommand command)
		{
			Command = command;
		}

		private bool Executed { get; set; }
		private ModbusCommand Command { get; }

		public override int Depth => 0;

		public override int FieldCount => Command.CommandDescriptor.Fields.Count;

		public override bool HasRows => true;

		public override bool IsClosed => Command.Connection.State == System.Data.ConnectionState.Closed;

		public override int RecordsAffected => 1;

		public override object this[int ordinal] => Schema.Rows[0][ordinal];

		public override object this[string name] => Schema.Rows[0][name];

		private DataTable Schema
		{
			get
			{
				if (_schema == null)
				{
					_schema = new DataTable();

					foreach (var field in Command.CommandDescriptor.Fields)
					{
						var column = new DataColumn
						{
							ColumnName = field.Name,
							DataType = ResolveType(field.DataType)
						};

						_schema.Columns.Add(column);
					}
				}

				return _schema;
			}
		}

		private Type ResolveType(ModbusFieldType type)
		{
			switch (type)
			{
				case ModbusFieldType.Bool:
					return typeof(bool);
				case ModbusFieldType.Byte:
					return typeof(byte);
				case ModbusFieldType.Short:
					return typeof(short);
				case ModbusFieldType.Int:
					return typeof(int);
				case ModbusFieldType.Long:
					return typeof(long);
				case ModbusFieldType.Float:
					return typeof(float);
				case ModbusFieldType.String:
					return typeof(string);
				default:
					throw new NotSupportedException();
			}
		}

		public override bool GetBoolean(int ordinal)
		{
			return Types.Convert<bool>(this[ordinal]);
		}

		public override byte GetByte(int ordinal)
		{
			return Types.Convert<byte>(this[ordinal]);
		}

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override char GetChar(int ordinal)
		{
			return Types.Convert<char>(this[ordinal]);
		}

		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override string GetDataTypeName(int ordinal)
		{
			return Schema.Columns[ordinal].DataType.ToFriendlyName();
		}

		public override DateTime GetDateTime(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override decimal GetDecimal(int ordinal)
		{
			return Types.Convert<decimal>(this[ordinal]);
		}

		public override double GetDouble(int ordinal)
		{
			return Types.Convert<double>(this[ordinal]);
		}

		public override IEnumerator GetEnumerator()
		{
			return Schema.Rows.GetEnumerator();
		}

		public override Type GetFieldType(int ordinal)
		{
			return Schema.Columns[ordinal].DataType;
		}

		public override float GetFloat(int ordinal)
		{
			return Types.Convert<float>(this[ordinal]);
		}

		public override Guid GetGuid(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override short GetInt16(int ordinal)
		{
			return Types.Convert<short>(this[ordinal]);
		}

		public override int GetInt32(int ordinal)
		{
			return Types.Convert<int>(this[ordinal]);
		}

		public override long GetInt64(int ordinal)
		{
			return Types.Convert<long>(this[ordinal]);
		}

		public override string GetName(int ordinal)
		{
			return Schema.Columns[ordinal].ColumnName;
		}

		public override int GetOrdinal(string name)
		{
			return Schema.Columns.IndexOf(Schema.Columns[name]);
		}

		public override string GetString(int ordinal)
		{
			return Types.Convert<string>(this[ordinal]);
		}

		public override object GetValue(int ordinal)
		{
			return Types.Convert<object>(this[ordinal]);
		}

		public override int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public override bool IsDBNull(int ordinal)
		{
			return this[ordinal] == null;
		}

		public override bool NextResult()
		{
			return false;
		}

		public override bool Read()
		{
			if (Executed)
				return false;

			Executed = true;

			ReadData();

			return true;
		}

		private void ReadData()
		{
			switch (Command.CommandDescriptor.Type)
			{
				case ModbusCommandType.Coil:
					ReadCoilData();
					break;
				case ModbusCommandType.Discrete:
					ReadDiscreteData();
					break;
				case ModbusCommandType.Holding:
					ReadHoldingData();
					break;
				case ModbusCommandType.Input:
					ReadInputData();
					break;
			}
		}

		private void ReadCoilData()
		{
			CreateRow(Command.Modbus.ReadCoils(Command.CommandDescriptor.Offset, Command.CommandDescriptor.Length));
		}

		private void ReadDiscreteData()
		{
			CreateRow(Command.Modbus.ReadDiscreteInputs(Command.CommandDescriptor.Offset, Command.CommandDescriptor.Length));
		}

		private void ReadHoldingData()
		{
			CreateRow(Command.Modbus.ReadHoldingRegisters(Command.CommandDescriptor.Offset, Command.CommandDescriptor.Length));
		}

		private void ReadInputData()
		{
			CreateRow(Command.Modbus.ReadInputRegisters(Command.CommandDescriptor.Offset, Command.CommandDescriptor.Length));
		}

		private void CreateRow(int[] values)
		{
			var dr = Schema.NewRow();

			foreach (DataColumn dc in _schema.Columns)
			{
				var field = Command.CommandDescriptor.Fields.FirstOrDefault(f => string.Compare(f.Name, dc.ColumnName, true) == 0);

				if (field == null || field.Offset >= values.Length)
					continue;

				dr[dc] = GetFieldValue(field, values);
			}

			Schema.Rows.Add(dr);
		}

		private void CreateRow(bool[] values)
		{
			var dr = Schema.NewRow();

			foreach (DataColumn dc in _schema.Columns)
			{
				var field = Command.CommandDescriptor.Fields.FirstOrDefault(f => string.Compare(f.Name, dc.ColumnName, true) == 0);

				if (field == null || field.Offset >= values.Length)
					continue;

				dr[dc] = values[field.Offset];
			}

			Schema.Rows.Add(dr);
		}

		private object GetFieldValue(ModbusCommandField field, int[] data)
		{
			switch (field.DataType)
			{
				case ModbusFieldType.String:
					if (field.Length + field.Offset >= data.Length)
						return null;

					var result = string.Empty;

					for (var i = field.Offset; i < field.Length + field.Offset; i++)
					{
						var j = BitConverter.GetBytes(data[i]);

						result += Types.Convert<char>(j[0]);
					}

					return result;
				case ModbusFieldType.Byte:
					if (field.Offset >= data.Length)
						return null;

					return Types.Convert<byte>(data[field.Offset]);
				case ModbusFieldType.Short:
					if (field.Offset >= data.Length)
						return null;

					return Types.Convert<short>(data[field.Offset]);
				case ModbusFieldType.Int:
					if (field.Offset + 2 >= data.Length)
						return null;

					var ints = new int[2];
					ints[0] = data[field.Offset];
					ints[1] = data[field.Offset + 1];

					return ModbusClient.ConvertRegistersToInt(ints);
				case ModbusFieldType.Float:
					if (field.Offset + 2 >= data.Length)
						return null;

					var floats = new int[2];
					floats[0] = data[field.Offset];
					floats[1] = data[field.Offset + 1];

					return ModbusClient.ConvertRegistersToFloat(floats);
				case ModbusFieldType.Bool:
					if (field.Offset >= data.Length)
						return null;

					return Types.Convert<bool>(data[field.Offset]);
				case ModbusFieldType.Long:
					if (field.Offset + 4 >= data.Length)
						return null;

					var longs = new int[2];
					longs[0] = data[field.Offset];
					longs[1] = data[field.Offset + 1];
					longs[2] = data[field.Offset + 2];
					longs[3] = data[field.Offset + 3];

					return ModbusClient.ConvertRegistersToLong(longs);
				default:
					throw new NotSupportedException();
			}
		}
	}
}
