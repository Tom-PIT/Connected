using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using TomPIT.Serialization;

namespace TomPIT.DataProviders.Modbus
{
	internal class ModbusCommand : DbCommand
	{
		private ModbusCommandDescriptor _command = null;
		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }
		protected override DbConnection DbConnection { get; set; }

		protected override DbParameterCollection DbParameterCollection => null;

		protected override DbTransaction DbTransaction { get; set; }

		public ModbusCommandDescriptor CommandDescriptor
		{
			get
			{
				if (_command == null && !string.IsNullOrWhiteSpace(CommandText))
					_command = Serializer.Deserialize<ModbusCommandDescriptor>(CommandText);

				return _command;
			}
		}

		public override void Cancel()
		{
			throw new NotSupportedException();
		}

		public override int ExecuteNonQuery()
		{
			Connection.Open();

			return CommandDescriptor.Type switch
			{
				ModbusCommandType.Discrete => ExecuteDiscrete(),
				ModbusCommandType.Input => ExecuteInput(),
				_ => throw new FunctionCodeNotSupportedException(),
			};
		}

		public override object ExecuteScalar()
		{
			throw new NotSupportedException();
		}

		public override void Prepare()
		{
			throw new NotSupportedException();
		}

		protected override DbParameter CreateDbParameter()
		{
			throw new NotSupportedException();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return new ModbusReader(this);
		}

		private int ExecuteInput()
		{
			var data = new List<int>();

			foreach (DbParameter parameter in Parameters)
			{
				switch (parameter.DbType)
				{
					case DbType.Boolean:
					case DbType.Byte:
					case DbType.Int16:
					case DbType.Int32:
						data.Add(Types.Convert<int>(parameter.Value));
						break;
					case DbType.Int64:
						var l = Types.Convert<long>(parameter.Value);

						data.AddRange(ModbusClient.ConvertLongToRegisters(l));
						break;
					case DbType.Single:
						var f = Types.Convert<float>(parameter.Value);

						data.AddRange(ModbusClient.ConvertFloatToRegisters(f));
						break;
					case DbType.String:
						var st = parameter.Value.ToString();

						for (var index = 0; index < st.Length; index += 2)
						{
							var a = Types.Convert<byte>(st[index]);
							var b = (byte)0;

							if (index + 1 < st.Length)
								b = Types.Convert<byte>(st[index + 1]);

							var s = new byte[] { a, 0, b, 0 };
							var si = BitConverter.ToInt32(s, 0);

							data.AddRange(ModbusClient.ConvertIntToRegisters(si));
						}
						break;
					default:
						break;
				}
			}

			Modbus.WriteMultipleRegisters(CommandDescriptor.Offset, data.ToArray());

			return data.Count;
		}

		private int ExecuteDiscrete()
		{
			var data = new List<bool>();

			foreach (DbParameter parameter in Parameters)
				data.Add(Types.Convert<bool>(parameter.Value));

			Modbus.WriteMultipleCoils(CommandDescriptor.Offset, data.ToArray());

			return data.Count;
		}

		public ModbusClient Modbus => (Connection as ModbusConnection).Client;
	}
}
