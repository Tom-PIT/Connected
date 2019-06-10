using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	internal class DataWriter : DataCommand, IDataWriter
	{
		public DataWriter(IExecutionContext context) : base(context)
		{
		}

		public void Execute()
		{
			try
			{
				Connection.Execute(CreateCommand());
			}
			finally
			{
				if (CloseConnection)
					Connection.Close();
			}
		}

		public T Execute<T>()
		{
			try
			{
				var command = CreateCommand();

				Connection.Execute(command);

				foreach (var parameter in command.Parameters)
				{
					if (parameter.Direction == System.Data.ParameterDirection.ReturnValue)
					{
						if (Types.TryConvert<T>(parameter.Value, out T r))
							return r;

						break;
					}
				}

				return default;
			}
			finally
			{
				if (CloseConnection)
					Connection.Close();
			}
		}

		public IDataParameter SetReturnValueParameter(string name)
		{
			var parameter = SetParameter(name, DBNull.Value);

			parameter.Direction = System.Data.ParameterDirection.ReturnValue;

			return parameter;
		}
	}
}