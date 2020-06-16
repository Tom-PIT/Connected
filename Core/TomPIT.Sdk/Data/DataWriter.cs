using System;
using System.Linq;
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
			try
			{
				Connection.Execute(CreateCommand());

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Close();
			}
		}

		public T Execute<T>()
		{
			try
			{
				var command = CreateCommand();

				Connection.Execute(command);
				var result = default(T);

				foreach (var parameter in command.Parameters)
				{
					if (parameter.Direction == System.Data.ParameterDirection.ReturnValue)
					{
						var par = Parameters.FirstOrDefault(f => string.Compare(parameter.Name, f.Name, true) == 0);

						if (par == null)
							continue;

						if (Types.TryConvert(parameter.Value, out T r))
						{
							par.Value = r;

							if (Types.Compare(result, default(T)))
								result = r;
						}
					}
				}

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				return result;
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
