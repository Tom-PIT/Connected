using System;
using System.Linq;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	internal class DataWriter : DataCommand, IDataWriter
	{
		public DataWriter(IMiddlewareContext context) : base(context)
		{
		}

		public int Execute()
		{
			try
			{
				var command = CreateCommand();
				var recordsAffected = Connection.Execute(command);

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				BindReturnValues(command);

				return recordsAffected;
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

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				BindReturnValues(command);

				foreach (var parameter in Parameters)
				{
					if (parameter.Direction == System.Data.ParameterDirection.ReturnValue
						&& Types.TryConvert(parameter.Value, out T r))
						return r;
				}

				return default;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Close();
			}
		}

		private void BindReturnValues(IDataCommandDescriptor command)
		{
			foreach (var parameter in command.Parameters)
			{
				if (parameter.Direction == System.Data.ParameterDirection.ReturnValue)
				{
					if (parameter.Value == DBNull.Value)
						continue;

					var par = Parameters.FirstOrDefault(f => string.Compare(parameter.Name, f.Name, true) == 0);

					if (par == null)
						continue;

					if (Types.TryConvert(parameter.Value, out object r, par.Type))
						par.Value = r;
				}
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
