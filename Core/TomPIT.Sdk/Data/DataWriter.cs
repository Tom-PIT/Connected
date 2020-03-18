using System;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	internal class DataWriter : DataCommand, IDataWriter
	{
		public DataWriter(IMiddlewareContext context) : base(context)
		{
		}

		public void Execute()
		{
			Connection.Execute(CreateCommand());
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

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				return default;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
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
